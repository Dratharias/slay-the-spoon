using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// Shared plumbing for the StS1 Ending enemies: static sprite visuals, ascension numbers, and a move cycle driven by
/// <see cref="EndingMoveBrain{TMove}"/> on the monster's own seeded RNG (never the vanilla AI or monster RNG).
/// </summary>
public abstract class Sts1EndingMonster : MonsterModel, IEndingMonster
{
	private Rng? _endingRng;

	public abstract string TexturePath { get; }
	public abstract Vector2 Origin { get; }
	public abstract Vector2 Size { get; }

	/// <summary>A real vanilla scene, so asset preloading succeeds; <see cref="EndingVisuals"/> builds the node instead.</summary>
	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/fallback");

	public override IEnumerable<string> AssetPaths => base.AssetPaths.Append(TexturePath);

	/// <summary>No FMOD event exists for these monsters (the run history screen would try to play it).</summary>
	public override bool HasDeathSfx => false;

	protected static int Ascension => EndingAscension.Current;

	/// <summary>Built once per combat from the run seed and the monster id, so every peer rolls the same values.</summary>
	protected Rng EndingRng => _endingRng ??= new Rng(CombatState.RunState.Rng.Seed, "SlaySpoon.Ending." + Id.Entry);

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		_ = EndingRng;
	}

	/// <summary>
	/// One move state per move, each followed by one shared branch that advances the brain once per roll. The branch
	/// is also the initial state, so the first move comes from the first Advance, at the first roll.
	/// </summary>
	protected MonsterMoveStateMachine BuildMachine<TMove>(EndingMoveBrain<TMove> brain, IReadOnlyDictionary<TMove, MoveState> moves)
		where TMove : struct, Enum
	{
		var branch = new EndingBranchState(() =>
		{
			TMove move = brain.Advance(EndingRng.NextInt(2));
			// One-based after Advance: "turn 3 -> Smash" reads like the checklist.
			SpoonLog.Info($"ending: {Id.Entry} turn {brain.Turn} -> {move}");
			return moves[move].Id;
		});
		var states = new List<MonsterState>();
		foreach (MoveState state in moves.Values)
		{
			state.FollowUpState = branch;
			states.Add(state);
		}
		states.Add(branch);
		return new MonsterMoveStateMachine(states, branch);
	}

	/// <summary>The living players an attack reached, once each, in target order (the same on every peer).</summary>
	protected static List<Creature> PlayersHit(AttackCommand attack) =>
		attack.Results.SelectMany(hit => hit).Select(r => r.Receiver).Where(c => c.IsPlayer && c.IsAlive).Distinct().ToList();

	/// <summary>
	/// StS1: when Shield or Spear dies, the players lose Surrounded and the survivor loses Back Attack. Vanilla
	/// SurroundedPower.AfterDeath (a player's power, so it runs before this monster's hook) has already turned the
	/// players toward the survivor; neither power is removed by vanilla code.
	/// </summary>
	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
		if (creature != Creature || wasRemovalPrevented) return;
		var survivors = CombatState.GetTeammatesOf(Creature).Where(c => c != Creature && c.IsAlive).ToList();
		if (survivors.Count == 0) return;
		foreach (Creature opponent in CombatState.GetOpponentsOf(Creature).ToList())
		{
			if (opponent.GetPower<SurroundedPower>() is { } surrounded) await PowerCmd.Remove(surrounded);
		}
		foreach (Creature survivor in survivors)
		{
			if (survivor.GetPower<BackAttackLeftPower>() is { } left) await PowerCmd.Remove(left);
			if (survivor.GetPower<BackAttackRightPower>() is { } right) await PowerCmd.Remove(right);
		}
	}
}
