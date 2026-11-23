using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// Gluttony: no effect of its own, it only explains the Spoon's growth on its hover tip (backlog: nothing on the Spoon
/// explained it). The growth itself lives in <see cref="SpoonMonster"/> (Grow, SyncStrength).
/// </summary>
public sealed class SpoonGluttonyPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(FeedingFrenzyPower);
}
