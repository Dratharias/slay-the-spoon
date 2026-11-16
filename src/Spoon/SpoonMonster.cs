using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Act4;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon (docs/superpowers/specs/2026-11-14-spoon-design.md §5): the Spoon act's only monster. It fights the
/// players who reached 300 max HP (the fighters); the others support them with Spoon-Feed and cannot hurt it or be
/// hurt by it. It plays a 4-turn cycle (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md): sets the table (Frail,
/// and the cutlery kit into every player's draw pile), Mouthful (Block from the HP it took), Banquet (Vulnerable, eats max
/// HP, heals) and Ladle Smash. Its hits grow with Appetite. It grows on the HP its hits take past Block (that much max HP,
/// without healing), and its Strength follows its max HP against the start (<see cref="SpoonRule.StrengthFor"/>).
/// </summary>
public sealed class SpoonMonster : Sts1EndingMonster
{
	public override string TexturePath => "res://SlaySpoon/images/spoon/spoon.png";

	/// <summary>No skeleton: the static sprite stays (EndingVisuals builds no Spine body, so no animator is made).</summary>
	public override string? SpineKey => null;

	public override Vector2 Origin => new(200, 593);
	public override Vector2 Size => new(400, 600);

	/// <summary>The handle and the lower half of the bowl: the whole sprite would put the intents off screen.</summary>
	public override Rect2 Hitbox => new(-100, -467, 200, 473);

	/// <summary>
	/// <see cref="SpoonRule.BossHp"/> for the fighters of the run. The creature reads this before the combat state exists,
	/// so it comes from the run's players (their max HP cannot change between here and <see cref="AfterAddedToRoom"/>).
	/// </summary>
	public override int MinInitialHp
	{
		get
		{
			if (Act4Routing.CurrentRun() is not { } run || run.Players.Count == 0) return SpoonRule.HpPerFighter;
			List<int> maxHps = run.Players.Select(p => p.Creature.MaxHp).ToList();
			return SpoonRule.BossHp(SpoonRule.FightersAtFight(maxHps).Count, maxHps.Count);
		}
	}

	public override int MaxInitialHp => MinInitialHp;

	/// <summary>Not saved: StS2 restarts the combat when a save loads, and the fighters are chosen again then.</summary>
	private HashSet<Player>? _fighters;

	/// <summary>Max HP grown this fight, for the sprite's size and left out of Strength. Not saved, like the fighters.</summary>
	private int _grown;

	/// <summary>How many times it set the table this fight; names the kit extras' Rng.</summary>
	private int _tablesSet;

	/// <summary>Max HP at fight start, the Strength reference.</summary>
	private int _startMaxHp;

	/// <summary>
	/// The Strength this monster gave itself from max HP. <see cref="SyncStrength"/> applies only the difference, so
	/// Strength the players take away (or anything else changes) is kept, and its own share never drifts.
	/// </summary>
	private int _strengthGiven;

	/// <summary>The sprite's tweens (<see cref="SpoonMotion"/>), found once the creature node exists.</summary>
	private SpoonMotion? _motion;

	/// <summary>Runs a visual-only step; a failure is logged and never reaches the combat.</summary>
	private void Animate(Action<SpoonMotion> step)
	{
		try
		{
			if ((_motion ??= SpoonMotion.For(Creature)) is { } motion) step(motion);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon: animation failed: {e.Message}");
		}
	}

	private async Task AnimateAsync(Func<SpoonMotion, Task> step)
	{
		try
		{
			if ((_motion ??= SpoonMotion.For(Creature)) is { } motion) await step(motion);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon: animation failed: {e.Message}");
		}
	}

	/// <summary>Set while the supporters are killed after the last fighter, so their own deaths don't recurse.</summary>
	private bool _losing;

	private static readonly Func<Creature, bool, int, Task> KillWithoutWinCheck =
		AccessTools.MethodDelegate<Func<Creature, bool, int, Task>>(
			AccessTools.Method(typeof(CreatureCmd), "KillWithoutCheckingWinCondition"));

	internal bool IsFighter(Player player) => FighterSet().Contains(player);

	private HashSet<Player> FighterSet()
	{
		if (_fighters != null) return _fighters;
		IReadOnlyList<Player> players = CombatState.Players;
		IReadOnlyList<int> indexes = SpoonRule.FightersAtFight(players.Select(p => p.Creature.MaxHp).ToList());
		_fighters = indexes.Select(i => players[i]).ToHashSet();
		SpoonLog.Info($"spoon: fight starts, fighters {string.Join(", ", indexes)} of {players.Count} players, HP {Creature.MaxHp}");
		return _fighters;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moves = new Dictionary<SpoonRule.SpoonMove, MoveState>
		{
			[SpoonRule.SpoonMove.SetTable] = new("SET_TABLE_MOVE", SetTableMove, new DebuffIntent(), new StatusIntent(1)),
			[SpoonRule.SpoonMove.Bouchee] = new("BOUCHEE_MOVE", BoucheeMove, new SingleAttackIntent(SpoonRule.BoucheeDamage), new DefendIntent()),
			[SpoonRule.SpoonMove.Banquet] = new("BANQUET_MOVE", BanquetMove, new SingleAttackIntent(SpoonRule.BanquetDamage), new DebuffIntent(true), new HealIntent()),
			[SpoonRule.SpoonMove.Ladle] = new("LADLE_MOVE", LadleMove, new SingleAttackIntent(SpoonRule.LadleDamage)),
		};
		return BuildMachine(new EndingMoveBrain<SpoonRule.SpoonMove>((turn, _, _) => SpoonRule.Move(turn), SpoonRule.SpoonMove.SetTable), moves);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		_ = FighterSet();
	}

	/// <summary>
	/// One Spoon-Feed in each supporter's hand. Here rather than in <see cref="AfterAddedToRoom"/>: generated cards are
	/// only added once the combat is in progress. Combat-only: a generated card never reaches the deck.
	/// </summary>
	public override async Task BeforeCombatStart()
	{
		await base.BeforeCombatStart();
		_startMaxHp = Creature.MaxHp;
		Animate(m => m.Idle());
		foreach (Player player in CombatState.Players)
		{
			if (IsFighter(player) || player.Creature.IsDead) continue;
			CardModel card = CombatState.CreateCard<SpoonSpoonFeed>(player);
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
		}
	}

	private IEnumerable<Creature> LivingFighters() =>
		CombatState.Players.Where(p => IsFighter(p) && p.Creature.IsAlive).Select(p => p.Creature).ToList();

	/// <summary>
	/// Sets the table: Frail on the fighters, and every living player's kit shuffled into their draw pile: one of each of
	/// the five spoons, plus <see cref="SpoonRule.ExtraSpoons"/> random ones for a 25+ card deck. The extras come from an
	/// Rng named after the run seed, the cycle and the player's slot, so every co-op client draws the same.
	/// </summary>
	private async Task SetTableMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SetTableAsync());
		var context = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<FrailPower>(context, LivingFighters(), SpoonRule.FrailAmount, Creature, null);
		_tablesSet++;
		IReadOnlyList<Player> players = CombatState.Players.ToList();
		for (int slot = 0; slot < players.Count; slot++)
		{
			Player player = players[slot];
			if (player.Creature.IsDead) continue;
			var counts = Enum.GetValues<SpoonRule.Cutlery>().ToDictionary(c => c, _ => 1);
			int deck = player.Deck.Cards.Count;
			int extras = SpoonRule.ExtraSpoons(deck);
			if (extras > 0)
			{
				var rng = new Rng(player.RunState.Rng.Seed, $"SlaySpoon.Spoon.Kit{_tablesSet}.{slot}");
				for (int i = 0; i < extras; i++) counts[SpoonRule.ExtraPick(rng.NextInt(counts.Count))]++;
			}
			await Deal<SpoonTeaspoon>(player, counts[SpoonRule.Cutlery.Teaspoon]);
			await Deal<SpoonWoodenSpoon>(player, counts[SpoonRule.Cutlery.Wooden]);
			await Deal<SpoonSkimmer>(player, counts[SpoonRule.Cutlery.Skimmer]);
			await Deal<SpoonMelonBaller>(player, counts[SpoonRule.Cutlery.Baller]);
			await Deal<SpoonStolenSpoonful>(player, counts[SpoonRule.Cutlery.StolenSpoonful]);
			SpoonLog.Info($"spoon: set the table for a {deck}-card deck: {string.Join(", ", counts.Select(c => $"{c.Key} {c.Value}"))}");
		}
		Animate(m => m.Recover());
		await AfterMove();
	}

	private static Task Deal<T>(Player player, int count) where T : CardModel =>
		count > 0
			? CardPileCmd.AddToCombatAndPreview<T>(player.Creature, PileType.Draw, count, null, CardPilePosition.Random)
			: Task.CompletedTask;

	/// <summary>Mouthful: a bite, and Block equal to the HP it took past the players' Block.</summary>
	private async Task BoucheeMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SwoopAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.BoucheeDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
		Animate(m => m.Recover());
		int unblocked = Unblocked(attack);
		if (unblocked > 0 && Creature.IsAlive)
			await CreatureCmd.GainBlock(Creature, unblocked, ValueProp.Unpowered, null);
		await Grow(unblocked);
		await AfterMove();
	}

	/// <summary>
	/// Banquet: a light hit (owner, 2026-11-16), Vulnerable on the fighters, each loses max HP unless Skimmed (lost, not
	/// gained), then it heals.
	/// </summary>
	private async Task BanquetMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.BanquetAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.BanquetDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
		await Grow(Unblocked(attack));
		var context = new ThrowingPlayerChoiceContext();
		List<Creature> fighters = LivingFighters().ToList();
		await PowerCmd.Apply<VulnerablePower>(context, fighters, SpoonRule.VulnerableAmount, Creature, null);
		int eaten = 0;
		foreach (Creature fighter in fighters)
		{
			if (!fighter.IsAlive || fighter.GetPower<SpoonSkimmedPower>() != null) continue;
			int before = fighter.MaxHp;
			await CreatureCmd.LoseMaxHp(context, fighter, SpoonRule.BanquetMaxHp, false);
			eaten += Math.Max(0, before - fighter.MaxHp);
		}
		int heal = SpoonRule.BanquetHeal(CombatState.Players.Count);
		if (Creature.IsAlive) await CreatureCmd.Heal(Creature, heal);
		Animate(m => m.Recover());
		SpoonLog.Info($"spoon: Banquet ate {eaten} max HP, healed {heal}");
		await AfterMove();
	}

	private async Task LadleMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SmashAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.LadleDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		Animate(m => m.Recover());
		await Grow(Unblocked(attack));
		await AfterMove();
	}

	private static int Unblocked(AttackCommand attack) =>
		attack.Results.SelectMany(hit => hit).Where(r => r.Receiver.IsPlayer).Sum(r => r.UnblockedDamage);

	/// <summary>After every move: Appetite (unless Sated), then the players' Skimmed ends.</summary>
	private async Task AfterMove()
	{
		await GainAppetite();
		foreach (Player player in CombatState.Players.ToList())
		{
			if (player.Creature.GetPower<SpoonSkimmedPower>() is { } skimmed) await PowerCmd.Remove(skimmed);
		}
	}

	/// <summary>Strength = <see cref="SpoonRule.StrengthFor"/>; only the change since the last sync is applied.</summary>
	internal async Task SyncStrength()
	{
		if (!Creature.IsAlive || _startMaxHp <= 0) return;
		int target = SpoonRule.StrengthFor(Creature.MaxHp, _startMaxHp, _grown);
		int delta = target - _strengthGiven;
		if (delta == 0) return;
		_strengthGiven = target;
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, delta, Creature, null);
		SpoonLog.Info($"spoon: max HP {Creature.MaxHp} (start {_startMaxHp}), Strength from max HP {target}");
	}

	/// <summary>
	/// +<see cref="SpoonRule.AppetitePerMove"/> after each move, so its first hit is at base; a Sated stack cancels one
	/// gain. Each gain also takes <see cref="SpoonRule.StolenBack"/> Appetite back from every player holding some.
	/// </summary>
	private async Task GainAppetite()
	{
		if (!Creature.IsAlive) return;
		var context = new ThrowingPlayerChoiceContext();
		if (Creature.GetPower<SpoonSatedPower>() is { Amount: > 0 } sated)
		{
			await PowerCmd.ModifyAmount(context, sated, -1, null, null);
			SpoonLog.Info("spoon: Sated, no Appetite this move");
			return;
		}
		int gain = SpoonRule.AppetitePerMove;
		foreach (Player player in CombatState.Players.ToList())
		{
			if (player.Creature.GetPower<SpoonAppetitePower>() is not { } held) continue;
			int taken = SpoonRule.StolenBack(held.Amount);
			if (taken <= 0) continue;
			await PowerCmd.ModifyAmount(context, held, -taken, Creature, null);
			gain += taken;
		}
		await PowerCmd.Apply<SpoonAppetitePower>(context, Creature, gain, Creature, null);
	}

	/// <summary>
	/// The HP its hits took from players past their Block becomes its max HP (SetMaxHp: no heal). Growth gives no
	/// Strength (<see cref="SpoonRule.StrengthFor"/>).
	/// </summary>
	private async Task Grow(int unblocked)
	{
		if (!Creature.IsAlive || unblocked <= 0) return;
		await CreatureCmd.SetMaxHp(Creature, Creature.MaxHp + unblocked);
		_grown += unblocked;
		Animate(m => m.Grow(_grown));
		SpoonLog.Info($"spoon: grew {unblocked} max HP (now {Creature.MaxHp})");
	}

	/// <summary>
	/// It never damages a supporter; a supporter's damage to it, their summons' included, is nothing. Its own hits are
	/// base + Strength, times Appetite (<see cref="SpoonAppetitePower"/>); the team's max HP no longer scales them
	/// (owner, 2026-11-16).
	/// </summary>
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == null || CombatState == null) return 1m;
		if (dealer == Creature && SpoonFight.OwnerOf(target) is { } victim && !IsFighter(victim)) return 0m;
		if (target == Creature && dealer != null && SpoonFight.OwnerOf(dealer) is { } attacker && !IsFighter(attacker)) return 0m;
		return 1m;
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
		ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
		if (target == Creature && result.UnblockedDamage > 0 && Creature.IsAlive) Animate(m => m.Recoil());
	}

	/// <summary>
	/// The fight is lost once every fighter is dead, even with supporters alive: they are killed (forced) here, inside the
	/// last fighter's death, so the vanilla kill that is running ends the combat and shows the game over once. The public
	/// Kill would run its own game over check as well.
	/// </summary>
	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
		if (creature == Creature && !wasRemovalPrevented) Animate(m => m.Topple());
		if (_losing || wasRemovalPrevented || !creature.IsPlayer || CombatState == null) return;
		IReadOnlyList<Player> players = CombatState.Players;
		if (players.Any(p => IsFighter(p) && p.Creature.IsAlive)) return;
		List<Creature> supporters = players.Where(p => !IsFighter(p) && p.Creature.IsAlive).Select(p => p.Creature).ToList();
		if (supporters.Count == 0) return;
		_losing = true;
		SpoonLog.Info($"spoon: every fighter is dead, the {supporters.Count} supporter(s) fall with them");
		foreach (Creature supporter in supporters)
		{
			await KillWithoutWinCheck(supporter, true, 0);
		}
	}
}
