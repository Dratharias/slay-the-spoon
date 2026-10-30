using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1's Mantra counter. Gain it only through <see cref="StanceCmd.GainMantra"/>, which enters Divinity at
/// <see cref="Core.StanceMath.MantraThreshold"/> and keeps the remainder.
/// </summary>
public sealed class Sts1MantraPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(RitualPower);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1DivinityStancePower>() };
}
