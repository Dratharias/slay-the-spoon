using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// Sated (Wooden Spoon, cutlery spec): on the Spoon, each stack cancels one of its Appetite gains. SpoonMonster reads and
/// spends it after each move.
/// </summary>
public sealed class SpoonSatedPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(SlowPower);
}
