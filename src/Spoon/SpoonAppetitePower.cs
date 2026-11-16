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

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon's Appetite (owner idea, 2026-11-16): it gains <see cref="SpoonRule.AppetitePerMove"/> after each move,
/// each stack adds <see cref="SpoonRule.AppetitePercent"/> % to its damage, and every Spoonful played on it removes
/// <see cref="SpoonRule.AppetiteBite"/>, a supporter's included (their damage is nothing, the feeding still counts).
/// </summary>
public sealed class SpoonAppetitePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(RavenousPower);

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource) =>
		dealer == Owner ? SpoonRule.AppetiteMultiplier(Amount, Owner.GetPower<SpoonSatedPower>() is { Amount: > 0 }) : 1m;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card is not SpoonSpoonful || cardPlay.Target != Owner) return;
		int removed = SpoonRule.AppetiteRemoved(Amount);
		if (removed <= 0) return;
		Flash();
		await PowerCmd.ModifyAmount(choiceContext, this, -removed, null, cardPlay.Card);
	}
}
