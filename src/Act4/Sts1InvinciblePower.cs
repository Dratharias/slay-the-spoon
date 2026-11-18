using MegaCrit.Sts2.Core.Entities.Cards;
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
	private readonly InvincibleBudget _budget = new();
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public Type IconSource => typeof(HardToKillPower);

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		// A re-apply mid-turn sees the depleted amount: the budget never lowers its cap.
		_budget.Applied(Amount);
		return Task.CompletedTask;
	}

	/// <summary>Sets the budget the power refills to each enemy turn, independent of the amount left when applied.</summary>
	public void SetTurnCap(int cap)
	{
		_budget.SetCap(cap);
		SetAmount(_budget.Remaining);
	}

	// Intent previews and block loss only; the authoritative clamp is ModifyHpLostAfterOsty.
	public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay) =>
		target == Owner ? HeartInvincibleRule.DamageCap(Amount, Owner.Block, props.HasFlag(ValueProp.Unblockable)) : decimal.MaxValue;

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource) =>
		target == Owner ? HeartInvincibleRule.ClampHpLoss(amount, Amount) : amount;

	// Vanilla calls this only on the modifiers that changed the amount, once per hit (as Intangible flashes).
	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
		ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == Owner)
		{
			_budget.Hit(result.UnblockedDamage);
			SetAmount(_budget.Remaining);
		}
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == Owner.Side)
		{
			_budget.TurnStart();
			SetAmount(_budget.Remaining);
		}
		return Task.CompletedTask;
	}
}
