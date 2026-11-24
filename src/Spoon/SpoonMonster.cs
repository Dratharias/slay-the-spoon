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
/// The Spoon (docs/superpowers/specs/2026-11-14-spoon-design.md §5): the Spoon act's only monster. Every player fights
/// it (owner, 2026-11-16: no more supporters; the team reaches the act together, <see cref="SpoonRule.TeamThreshold"/>).
/// It plays a 4-turn cycle (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md): sets the table (Frail,
/// and the cutlery kit into every player's draw pile), Mouthful (Block from the HP it took), Banquet (Vulnerable, eats max
/// HP, heals) and Ladle Smash. Its hits grow with Appetite. It grows by the HP its hits take past Block (max HP,
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
	/// <see cref="SpoonRule.HpPerPlayer"/>; in co-op the vanilla scaling multiplies it by the player count and the act 3 boss factor.
	/// </summary>
	public override int MinInitialHp => SpoonRule.HpPerPlayer;

	public override int MaxInitialHp => MinInitialHp;

	/// <summary>The Spoon is no Ending monster: its streams are <c>SlaySpoon.Spoon.*</c>.</summary>
	protected override string RngFamily => "SlaySpoon.Spoon.";

	/// <summary>Max HP grown this fight, for the sprite's size. Not saved: StS2 restarts the combat when a save loads.</summary>
	private int _grown;

	/// <summary>Max HP stolen by Ladle Smash, also used for the sprite's size.</summary>
	private int _stolen;

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

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moves = new Dictionary<SpoonRule.SpoonMove, MoveState>
		{
			[SpoonRule.SpoonMove.SetTable] = new("SET_TABLE_MOVE", SetTableMove, new DebuffIntent(), new StatusIntent(1)),
			[SpoonRule.SpoonMove.Bouchee] = new("BOUCHEE_MOVE", BoucheeMove, new SingleAttackIntent(SpoonRule.BoucheeDamage), new DefendIntent()),
			[SpoonRule.SpoonMove.Banquet] = new("BANQUET_MOVE", BanquetMove, new SpoonBanquetIntent(), new DebuffIntent(true), new HealIntent()),
			[SpoonRule.SpoonMove.Ladle] = new("LADLE_MOVE", LadleMove, new SingleAttackIntent(SpoonRule.LadleDamage)),
		};
		return BuildMachine(new EndingMoveBrain<SpoonRule.SpoonMove>((turn, _, _) => SpoonRule.Move(turn), SpoonRule.SpoonMove.SetTable), moves);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		SpoonLog.Info($"spoon: fight starts, {CombatState.Players.Count} player(s), HP {Creature.MaxHp}");
	}

	/// <summary>
	/// Turns the Spoonfuls into Stolen Spoonfuls. Here rather than in <see cref="AfterAddedToRoom"/>: the combat piles only
	/// exist once the combat is in progress.
	/// </summary>
	public override async Task BeforeCombatStart()
	{
		await base.BeforeCombatStart();
		_startMaxHp = Creature.MaxHp;
		Animate(m => m.Idle());
		await PowerCmd.Apply<SpoonGluttonyPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
		await TransformSpoonfuls();
	}

	/// <summary>
	/// For this fight every Spoonful becomes a Stolen Spoonful, upgrade kept (owner, 2026-11-16: the two are merged).
	/// Only the combat copies change (a combat pile is not the deck, so Eternal does not block it): the deck keeps its
	/// Spoonful after the fight.
	/// </summary>
	private async Task TransformSpoonfuls()
	{
		foreach (Player player in CombatState.Players.ToList())
		{
			if (player.PlayerCombatState is not { } combat) continue;
			foreach (SpoonSpoonful spoonful in combat.AllCards.OfType<SpoonSpoonful>().ToList())
			{
				CardModel stolen = CombatState.CreateCard<SpoonStolenSpoonful>(player);
				if (spoonful.IsUpgraded) CardCmd.Upgrade(stolen);
				await CardCmd.Transform(spoonful, stolen);
				SpoonLog.Info($"spoon: player {player.NetId} Spoonful -> Stolen Spoonful{(spoonful.IsUpgraded ? "+" : "")}");
			}
		}
	}

	private IEnumerable<Creature> LivingPlayers() =>
		CombatState.Players.Where(p => p.Creature.IsAlive).Select(p => p.Creature).ToList();

	/// <summary>
	/// Sets the table: Frail on every living player, and every living player's kit shuffled into their draw pile: one of each of
	/// the four spoons, plus <see cref="SpoonRule.ExtraSpoons"/> random ones for a 25+ card deck. The extras come from an
	/// Rng named after the run seed, the cycle and the player's slot, so every co-op client draws the same.
	/// </summary>
	private async Task SetTableMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SetTableAsync());
		var context = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<FrailPower>(context, LivingPlayers(), SpoonRule.FrailAmount, Creature, null);
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
			SpoonLog.Info($"spoon: set the table for a {deck}-card deck: {string.Join(", ", counts.Select(c => $"{c.Key} {c.Value}"))}");
		}
		Animate(m => m.Recover());
		await AfterMove(tableSet: SpoonRule.GainsAppetite(SpoonRule.SpoonMove.SetTable));
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
	/// Banquet: fixed unblockable HP loss and max HP loss unless Skimmed, then Vulnerable and healing.
	/// </summary>
	private async Task BanquetMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.BanquetAsync());
		var context = new ThrowingPlayerChoiceContext();
		List<Creature> eaters = LivingPlayers().ToList();
		int eaten = 0;
		int unblocked = 0;
		foreach (Creature target in eaters)
		{
			if (!target.IsAlive) continue;
			if (target.GetPower<SpoonSkimmedPower>() is { } skimmed)
			{
				await PowerCmd.Remove(skimmed);
				continue;
			}
			BanquetTarget = target;
			try
			{
				var results = await CreatureCmd.Damage(context, target, SpoonRule.BanquetDamage,
					ValueProp.Unblockable | ValueProp.Unpowered, Creature, null, null);
				int lost = results.Sum(r => r.UnblockedDamage);
				unblocked += lost;
				_pierced[target] = lost;
			}
			finally { BanquetTarget = null; }
			if (!target.IsAlive || !Creature.IsAlive) continue;
			int before = target.MaxHp;
			await CreatureCmd.LoseMaxHp(context, target, SpoonRule.BanquetMaxHp, false);
			eaten += Math.Max(0, before - target.MaxHp);
		}
		await Grow(unblocked);
		await PowerCmd.Apply<VulnerablePower>(context, eaters.Where(f => f.IsAlive).ToList(),
			SpoonRule.VulnerableAmount, Creature, null);
		int heal = SpoonRule.BanquetHeal(CombatState.Players.Count);
		if (Creature.IsAlive) await CreatureCmd.Heal(Creature, heal);
		Animate(m => m.Recover());
		SpoonLog.Info($"spoon: Banquet ate {eaten} max HP, healed {heal}");
		await AfterMove();
	}

	/// <summary>
	/// Ladle Smash: the hit, its growth, then it steals max HP from each living player hit: a quarter of the damage past
	/// their Block (<see cref="SpoonRule.LadleMaxHpSteal"/>). Like all max HP gains, this contributes 1 Strength per 5 above the combat baseline.
	/// </summary>
	private async Task LadleMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SmashAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.LadleDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		Animate(m => m.Recover());
		await Grow(Unblocked(attack));
		var context = new ThrowingPlayerChoiceContext();
		int stolen = 0;
		foreach ((Creature victim, int damage) in _pierced.ToList())
		{
			if (!victim.IsAlive || !Creature.IsAlive) continue;
			int before = victim.MaxHp;
			await CreatureCmd.LoseMaxHp(context, victim, SpoonRule.LadleMaxHpSteal(damage, before), false);
			stolen += Math.Max(0, before - victim.MaxHp);
		}
		if (stolen > 0 && Creature.IsAlive)
		{
			await CreatureCmd.SetMaxHp(Creature, Creature.MaxHp + stolen);
			_stolen += stolen;
			Animate(m => m.Grow(_grown + _stolen));
			SpoonLog.Info($"spoon: Ladle stole {stolen} max HP (now {Creature.MaxHp})");
			await SyncStrength();
		}
		await AfterMove();
	}

	/// <summary>The HP the hits took past Block; also records, per player, damage for Ladle theft; even zero-damage hits are recorded.</summary>
	private int Unblocked(AttackCommand attack)
	{
		int total = 0;
		foreach (DamageResult r in attack.Results.SelectMany(hit => hit).Where(r => r.Receiver.IsPlayer))
		{
			total += r.UnblockedDamage;
			_pierced[r.Receiver] = _pierced.GetValueOrDefault(r.Receiver) + r.UnblockedDamage;
		}
		return total;
	}

	/// <summary>Per player creature, the HP this move's hits took past their Block; cleared after each move.</summary>
	private readonly Dictionary<Creature, int> _pierced = new();

	/// <summary>Scopes fixed-damage hooks to the current Banquet victim, excluding other damage and max HP loss.</summary>
	internal Creature? BanquetTarget { get; private set; }

	/// <summary>
	/// After every move: one Sated turn is spent, hit players lose Appetite to the Spoon, it gains its own after Sets
	/// the Table, Skimmed is consumed by Banquet itself.
	/// </summary>
	private async Task AfterMove(bool tableSet = false)
	{
		await GainAppetite(tableSet);
		_pierced.Clear();
	}

	/// <summary>Strength = <see cref="SpoonRule.StrengthFor"/>; only the change since the last sync is applied.</summary>
	internal async Task SyncStrength()
	{
		if (!Creature.IsAlive || _startMaxHp <= 0) return;
		int target = SpoonRule.StrengthFor(Creature.MaxHp, _startMaxHp);
		int delta = target - _strengthGiven;
		if (delta == 0) return;
		_strengthGiven = target;
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, delta, Creature, null);
		SpoonLog.Info($"spoon: max HP {Creature.MaxHp} (start {_startMaxHp}), Strength from max HP {target}");
	}

	/// <summary>
	/// +<see cref="SpoonRule.AppetitePerMove"/> after Sets the Table only (<see cref="SpoonRule.GainsAppetite"/>), so its
	/// first hit is at base. A Sated stack (its Appetite did nothing this turn) is spent first. A move whose hits
	/// touched a player, even through full Block, takes <see cref="SpoonRule.StolenBack"/> Appetite from them, down to negative.
	/// </summary>
	private async Task GainAppetite(bool tableSet)
	{
		if (!Creature.IsAlive) return;
		var context = new ThrowingPlayerChoiceContext();
		if (Creature.GetPower<SpoonSatedPower>() is { Amount: > 0 } sated)
		{
			await PowerCmd.ModifyAmount(context, sated, -1, null, null);
			SpoonLog.Info("spoon: Sated turn spent, Appetite did nothing this move");
		}
		int gain = tableSet ? SpoonRule.AppetiteGain(CombatState.Players.Count(p => p.Creature.IsAlive)) : 0;
		foreach (Player player in CombatState.Players.ToList())
		{
			if (!player.Creature.IsAlive) continue;
			int taken = SpoonRule.StolenBack(_pierced.ContainsKey(player.Creature));
			if (taken <= 0) continue;
			await PowerCmd.Apply<SpoonAppetitePower>(context, player.Creature, -taken, Creature, null);
			gain += taken;
		}
		if (gain != 0) await PowerCmd.Apply<SpoonAppetitePower>(context, Creature, gain, Creature, null);
	}

	/// <summary>
	/// The HP its hits took from players past their Block becomes its max HP (SetMaxHp: no heal), then updates
	/// Strength (<see cref="SpoonRule.StrengthFor"/>).
	/// </summary>
	private async Task Grow(int unblocked)
	{
		if (!Creature.IsAlive || unblocked <= 0) return;
		int growth = SpoonRule.GrowthFromDamage(unblocked);
		if (growth == 0) return;
		await CreatureCmd.SetMaxHp(Creature, Creature.MaxHp + growth);
		_grown += growth;
		Animate(m => m.Grow(_grown));
		SpoonLog.Info($"spoon: grew {growth} max HP (now {Creature.MaxHp})");
		await SyncStrength();
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
		ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
		if (target == Creature && result.UnblockedDamage > 0 && Creature.IsAlive) Animate(m => m.Recoil());
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
		if (creature == Creature && !wasRemovalPrevented) Animate(m => m.Topple());
	}
}
