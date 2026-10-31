using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>StS1 Spire Shield: the left half of the Act 4 elite, standing behind the players.</summary>
public sealed class Sts1SpireShield : Sts1EndingMonster
{
	public override string TexturePath => "res://SlaySpoon/images/ending/shield.png";
	public override Vector2 Origin => new(127, 271);
	public override Vector2 Size => new(304, 295);

	public override int MinInitialHp => EndingNumbers.ShieldHp(Ascension);
	public override int MaxInitialHp => MinInitialHp;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moves = new Dictionary<ShieldMove, MoveState>
		{
			[ShieldMove.Bash] = new("BASH_MOVE", BashMove, new SingleAttackIntent(EndingNumbers.ShieldBash(Ascension)), new DebuffIntent()),
			[ShieldMove.Fortify] = new("FORTIFY_MOVE", FortifyMove, new DefendIntent()),
			[ShieldMove.Smash] = new("SMASH_MOVE", SmashMove, new SingleAttackIntent(EndingNumbers.ShieldSmash(Ascension)), new DefendIntent()),
		};
		return BuildMachine(new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Bash), moves);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, EndingNumbers.ShieldArtifact(Ascension), Creature, null);
		await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
	}

	private async Task BashMove(IReadOnlyList<Creature> targets)
	{
		var attack = await DamageCmd.Attack(EndingNumbers.ShieldBash(Ascension)).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		foreach (Creature player in PlayersHit(attack))
		{
			bool hasOrbSlots = player.Player!.PlayerCombatState?.OrbQueue.Capacity > 0;
			// Rolled for every player, orbs or not, so every peer draws the same number of values.
			if (EndingMoveRule.BashDebuffFor(hasOrbSlots, EndingRng.NextInt(2)) == BashDebuff.Focus)
				await PowerCmd.Apply<FocusPower>(new ThrowingPlayerChoiceContext(), player, -1m, Creature, null);
			else
				await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), player, -1m, Creature, null);
		}
	}

	private async Task FortifyMove(IReadOnlyList<Creature> targets)
	{
		foreach (Creature ally in CombatState.GetTeammatesOf(Creature).Where(c => c.IsAlive).ToList())
			await CreatureCmd.GainBlock(ally, EndingNumbers.FortifyBlock, ValueProp.Move, null);
	}

	private List<int>? _smashHits;

	public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount,
		ValueProp props, Creature? dealer, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
	{
		if (_smashHits != null && dealer == Creature && props.IsPoweredAttack())
			_smashHits.Add(Math.Max(0, (int)amount));
		return Task.CompletedTask;
	}

	private async Task SmashMove(IReadOnlyList<Creature> targets)
	{
		_smashHits = new List<int>();
		try
		{
			await DamageCmd.Attack(EndingNumbers.ShieldSmash(Ascension)).FromMonster(this)
				.WithHitFx("vfx/vfx_attack_blunt").Execute(null);
			int block = EndingNumbers.ShieldSmashBlock(Ascension, _smashHits);
			if (block > 0) await CreatureCmd.GainBlock(Creature, block, ValueProp.Move, null);
		}
		finally { _smashHits = null; }
	}
}
