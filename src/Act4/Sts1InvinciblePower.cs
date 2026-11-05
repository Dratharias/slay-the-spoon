using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>Limits HP loss after Block; resets before the enemy turn, including its retaliation damage.</summary>
public sealed class Sts1InvinciblePower : PowerModel, IBorrowedIcon
{
	// Not saved: StS2 restarts the combat when a save loads, so AfterApplied runs again.
	private int _turnCap;
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public Type IconSource => typeof(HardToKillPower);

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		// A re-apply mid-turn sees the depleted amount: never lower the cap below what it already was.
		_turnCap = Math.Max(_turnCap, Amount);
		return Task.CompletedTask;
	}

	/// <summary>Sets the budget the power refills to each enemy turn, independent of the amount left when applied.</summary>
	public void SetTurnCap(int cap)
	{
		_turnCap = cap;
		SetAmount(cap);
	}

	// Intent previews and block loss only; the authoritative clamp is ModifyHpLostAfterOsty.
	public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource) =>
		target == Owner ? HeartInvincibleRule.DamageCap(Amount, Owner.Block, props.HasFlag(ValueProp.Unblockable)) : decimal.MaxValue;

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != Owner || amount <= Amount) return amount;
		Flash();
		return Amount;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
		ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == Owner) SetAmount(HeartInvincibleRule.AfterHpLoss(Amount, result.UnblockedDamage));
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == Owner.Side) SetAmount(_turnCap);
		return Task.CompletedTask;
	}
}
