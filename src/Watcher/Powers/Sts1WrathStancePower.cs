using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1's Wrath stance: the holder's card attacks deal double damage, and enemy attacks on the holder deal double damage.
/// Teammates are untouched in either direction. Entered and left only through <see cref="StanceCmd"/>.
/// </summary>
public sealed class Sts1WrathStancePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(EnragePower);

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (dealer == Owner && cardSource != null)
		{
			return StanceMath.DealtMultiplier(Stance.Wrath);
		}
		if (target == Owner && dealer != null && dealer.Side != Owner.Side)
		{
			return StanceMath.ReceivedMultiplier(Stance.Wrath);
		}
		return 1m;
	}
}
