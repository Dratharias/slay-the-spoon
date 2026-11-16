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
/// Appetite (owner idea, 2026-11-16): each stack adds <see cref="SpoonRule.AppetitePercent"/> % to its holder's damage,
/// and it can go negative (<see cref="SpoonRule.AppetiteMultiplier"/>). The Spoon gains
/// <see cref="SpoonRule.AppetitePerMove"/> after Sets the Table; the cards that remove it apply a negative amount, so
/// they work even while it holds none.
/// </summary>
public sealed class SpoonAppetitePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool AllowNegative => true;

	public Type IconSource => typeof(RavenousPower);

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource) =>
		dealer == Owner ? SpoonRule.AppetiteMultiplier(Amount, Owner.GetPower<SpoonSatedPower>() is { Amount: > 0 }) : 1m;
}
