using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>StS1 Corrupt Heart: the Act 4 boss. Invincible caps its HP loss per turn; Beat of Death punishes every card.</summary>
public sealed class Sts1CorruptHeart : Sts1EndingMonster
{
	public override string TexturePath => "res://SlaySpoon/images/ending/heart.png";
	public override Vector2 Origin => new(189, 830);
	public override Vector2 Size => new(560, 877);

	/// <summary>
	/// StS1's hitbox (476 x 410, centred 30 px right of the ground point, bottom 30 px below it). The vents above it
	/// reach the top of the screen, so the whole sprite would put the intents off screen.
	/// </summary>
	public override Rect2 Hitbox => new(-208, -380, 476, 410);

	public override int MinInitialHp => EndingNumbers.HeartHp(Ascension);
	public override int MaxInitialHp => MinInitialHp;

	/// <summary>Not saved: StS2 restarts the combat when a save loads, so the count restarts with it.</summary>
	private int _buffsDone;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		var moves = new Dictionary<HeartMove, MoveState>
		{
			[HeartMove.Debilitate] = new("DEBILITATE_MOVE", DebilitateMove, new DebuffIntent(strong: true), new StatusIntent(StatusCards)),
			[HeartMove.BloodShots] = new("BLOOD_SHOTS_MOVE", BloodShotsMove,
				new MultiAttackIntent(EndingNumbers.HeartBloodShot(Ascension), EndingNumbers.HeartBloodShotHits(Ascension))),
			[HeartMove.Echo] = new("ECHO_MOVE", EchoMove, new SingleAttackIntent(EndingNumbers.HeartEcho(Ascension))),
			[HeartMove.Buff] = new("BUFF_MOVE", BuffMove, new BuffIntent()),
		};
		return BuildMachine(new EndingMoveBrain<HeartMove>(EndingMoveRule.Heart, HeartMove.Debilitate), moves);
	}

	/// <summary>Dazed, Slimed, Wound, Burn and Void, one each.</summary>
	private const int StatusCards = 5;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		// The budget follows ascension and player count only; the Monster HP percent never changes it.
		int cap = HeartInvincibleRule.InitialCap(Ascension, CombatState.Players.Count);
		var invincible = await PowerCmd.Apply<Sts1InvinciblePower>(new ThrowingPlayerChoiceContext(), Creature, cap, Creature, null);
		invincible?.SetTurnCap(cap);
		await PowerCmd.Apply<Sts1BeatOfDeathPower>(new ThrowingPlayerChoiceContext(), Creature, EndingNumbers.HeartBeatOfDeath(Ascension), Creature, null);
	}

	private async Task DebilitateMove(IReadOnlyList<Creature> targets)
	{
		var players = targets.Where(c => c.IsPlayer && c.IsAlive).ToList();
		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), players, EndingNumbers.DebilitateAmount, Creature, null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), players, EndingNumbers.DebilitateAmount, Creature, null);
		await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), players, EndingNumbers.DebilitateAmount, Creature, null);
		// StS1: MakeTempCardInDrawPileAction(..., randomSpot: true) for each card, so each lands anywhere in the draw pile.
		foreach (Creature player in players)
		{
			var owner = player.Player!;
			CardModel[] cards =
			[
				CombatState.CreateCard<Dazed>(owner),
				CombatState.CreateCard<Slimed>(owner),
				CombatState.CreateCard<Wound>(owner),
				CombatState.CreateCard<Burn>(owner),
				CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Void>(owner),
			];
			await EndingDrawPile.ShuffleIn(owner, cards, EndingRng);
		}
	}

	private async Task BloodShotsMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EndingNumbers.HeartBloodShot(Ascension)).FromMonster(this)
			.WithHitCount(EndingNumbers.HeartBloodShotHits(Ascension))
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task EchoMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(EndingNumbers.HeartEcho(Ascension)).FromMonster(this)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task BuffMove(IReadOnlyList<Creature> targets)
	{
		int gain = HeartBuffRule.StrengthGain(Creature.GetPowerAmount<StrengthPower>());
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, gain, Creature, null);

		var (kind, amount) = HeartBuffRule.Extra(_buffsDone);
		SpoonLog.Info($"ending: {Id.Entry} buff {_buffsDone + 1}: Strength +{gain}, {kind} +{amount}");
		switch (kind)
		{
			case HeartBuffKind.Artifact:
				await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null);
				break;
			case HeartBuffKind.BeatOfDeath:
				await PowerCmd.Apply<Sts1BeatOfDeathPower>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null);
				break;
			case HeartBuffKind.PainfulStabs:
				// Vanilla PainfulStabsPower adds Amount Wounds per unblocked powered hit to that player's discard pile.
				await PowerCmd.Apply<PainfulStabsPower>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null);
				break;
			default:
				await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, amount, Creature, null);
				break;
		}
		_buffsDone++;
	}
}
