using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// StS1 Spire Spear: the right half of the Act 4 elite. Like vanilla Rocket, it surrounds the players and holds
/// the right-hand Back Attack.
/// </summary>
public sealed class Sts1SpireSpear : Sts1EndingMonster
{
	public override string TexturePath => "res://SlaySpoon/images/ending/spear.png";
	public override Vector2 Origin => new(270, 284);
	public override Vector2 Size => new(437, 316);

	public override int MinInitialHp => EndingNumbers.SpearHp(Ascension);
	public override int MaxInitialHp => MinInitialHp;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moves = new Dictionary<SpearMove, MoveState>
		{
			[SpearMove.BurnStrike] = new("BURN_STRIKE_MOVE", BurnStrikeMove,
				new MultiAttackIntent(EndingNumbers.SpearBurnStrike(Ascension), EndingNumbers.SpearBurnStrikeHits),
				new StatusIntent(EndingNumbers.SpearBurns)),
			[SpearMove.Piercer] = new("PIERCER_MOVE", PiercerMove, new BuffIntent()),
			[SpearMove.Skewer] = new("SKEWER_MOVE", SkewerMove,
				new MultiAttackIntent(EndingNumbers.SpearSkewer(Ascension), EndingNumbers.SpearSkewerHits(Ascension))),
		};
		return BuildMachine(new EndingMoveBrain<SpearMove>(EndingMoveRule.Spear, SpearMove.BurnStrike), moves);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, EndingNumbers.SpearArtifact(Ascension), Creature, null);
		await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
		await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), CombatState.GetOpponentsOf(Creature), 1m, Creature, null);
	}

	private async Task BurnStrikeMove(IReadOnlyList<Creature> targets)
	{
		var attack = await DamageCmd.Attack(EndingNumbers.SpearBurnStrike(Ascension)).FromMonster(this)
			.WithHitCount(EndingNumbers.SpearBurnStrikeHits)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		// StS1 A18: MakeTempCardInDrawPileAction(..., randomSpot: false), so the Burns go on top of the draw pile.
		// No creator: MonsterDamageRule treats these Burns as enemy statuses.
		if (EndingNumbers.SpearBurnsToDrawPile(Ascension))
			await CardPileCmd.AddToCombatAndPreview<Burn>(PlayersHit(attack), PileType.Draw, EndingNumbers.SpearBurns, null, CardPilePosition.Top);
		else
			await CardPileCmd.AddToCombatAndPreview<Burn>(PlayersHit(attack), PileType.Discard, EndingNumbers.SpearBurns, null);
	}

	private async Task PiercerMove(IReadOnlyList<Creature> targets)
	{
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(),
			CombatState.GetTeammatesOf(Creature).Where(c => c.IsAlive).ToList(), EndingNumbers.PiercerStrength, Creature, null);
	}

	private async Task SkewerMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EndingNumbers.SpearSkewer(Ascension)).FromMonster(this)
			.WithHitCount(EndingNumbers.SpearSkewerHits(Ascension))
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}
}
