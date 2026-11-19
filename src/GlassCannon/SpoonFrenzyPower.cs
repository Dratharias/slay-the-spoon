using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.GlassCannon;

/// <summary>
/// Glass Cannon's Frenzy: +10 % powered attack damage per card played (the holder's and their Osty's hits), and Strength
/// equal to the streak at the end of each turn without a Block Skill. A Block Skill resets the stacks and the streak;
/// Strength already gained stays. A Buff, so Artifact ignores it. Rules in <see cref="GlassCannonRule"/>.
/// </summary>
public sealed class SpoonFrenzyPower : PowerModel, IBorrowedIcon
{
	private FrenzyState _state = FrenzyState.Start;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(EnragePower);

	/// <summary>The bonus percent, as vanilla Slow shows it.</summary>
	public override int DisplayAmount => _state.Stacks * 10;

	public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.Player == null || cardPlay.Player != Owner.Player) return Task.CompletedTask;
		_state = GlassCannonRule.CardPlayed(_state, GlassCannonBlock.BreaksFrenzy(cardPlay.Card));
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (dealer == null || !props.IsPoweredAttack()) return 1m;
		if (dealer != Owner && (Owner.Player == null || dealer != Owner.Player.Osty)) return 1m;
		return GlassCannonRule.DamageMultiplier(_state.Stacks);
	}

	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		if (_state.Stacks > 0) Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner)) return;
		(_state, int strength) = GlassCannonRule.TurnEnded(_state);
		if (strength <= 0 || Owner.IsDead) return;
		Flash();
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, strength, Owner, null);
	}
}
