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
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Act4;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon (docs/superpowers/specs/2026-11-14-spoon-design.md §5): the Spoon act's only monster. It fights the
/// players who reached 300 max HP (the fighters); the others support them with Spoon-Feed and cannot hurt it or be
/// hurt by it. Its hits grow with Appetite, which Spoonful eats away, and Mouthful eats max HP to heal it. It grows on the HP its
/// hits take past Block: that much max HP (without healing), and 1 Strength per 10 max HP grown (owner, 2026-11-16).
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

	/// <summary>Max HP grown this fight, for the Strength remainder. Not saved, like the fighters.</summary>
	private int _grown;

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
			[SpoonRule.SpoonMove.Bouchee] = new("BOUCHEE_MOVE", BoucheeMove, new SingleAttackIntent(SpoonRule.BoucheeDamage), new DebuffIntent()),
			[SpoonRule.SpoonMove.Ladle] = new("LADLE_MOVE", LadleMove, new SingleAttackIntent(SpoonRule.LadleDamage)),
		};
		return BuildMachine(new EndingMoveBrain<SpoonRule.SpoonMove>((turn, _, _) => SpoonRule.Move(turn), SpoonRule.SpoonMove.Bouchee), moves);
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
		Animate(m => m.Idle());
		foreach (Player player in CombatState.Players)
		{
			if (IsFighter(player) || player.Creature.IsDead) continue;
			CardModel card = CombatState.CreateCard<SpoonSpoonFeed>(player);
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
		}
	}

	private async Task BoucheeMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SwoopAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.BoucheeDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
		Animate(m => m.Recover());
		await Grow(attack);
		await GainAppetite();
		var context = new ThrowingPlayerChoiceContext();
		int eaten = 0;
		foreach (Creature fighter in PlayersHit(attack))
		{
			int before = fighter.MaxHp;
			await CreatureCmd.LoseMaxHp(context, fighter, SpoonRule.BoucheeMaxHp, false);
			eaten += Math.Max(0, before - fighter.MaxHp);
		}
		if (eaten > 0 && Creature.IsAlive) await CreatureCmd.Heal(Creature, eaten);
		SpoonLog.Info($"spoon: Mouthful ate {eaten} max HP");
	}

	private async Task LadleMove(IReadOnlyList<Creature> targets)
	{
		await AnimateAsync(m => m.SmashAsync());
		AttackCommand attack = await DamageCmd.Attack(SpoonRule.LadleDamage).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		Animate(m => m.Recover());
		await Grow(attack);
		await GainAppetite();
	}

	/// <summary>After each move, so its first hit is at base.</summary>
	private Task GainAppetite() => Creature.IsAlive
		? PowerCmd.Apply<SpoonAppetitePower>(new ThrowingPlayerChoiceContext(), Creature, SpoonRule.AppetitePerMove, Creature, null)
		: Task.CompletedTask;

	/// <summary>
	/// The HP the hit took from players past their Block becomes the Spoon's max HP (SetMaxHp: no heal, unlike GainMaxHp;
	/// only the max HP Mouthful eats heals it), with 1 Strength per <see cref="SpoonRule.MaxHpPerStrength"/> grown.
	/// </summary>
	private async Task Grow(AttackCommand attack)
	{
		if (!Creature.IsAlive) return;
		int unblocked = attack.Results.SelectMany(hit => hit).Where(r => r.Receiver.IsPlayer).Sum(r => r.UnblockedDamage);
		if (unblocked <= 0) return;
		await CreatureCmd.SetMaxHp(Creature, Creature.MaxHp + unblocked);
		int strength = SpoonRule.StrengthFromGrowth(_grown, unblocked);
		_grown += unblocked;
		Animate(m => m.Grow(_grown));
		if (strength > 0)
			await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, strength, Creature, null);
		SpoonLog.Info($"spoon: grew {unblocked} max HP (now {Creature.MaxHp}), +{strength} Strength");
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
