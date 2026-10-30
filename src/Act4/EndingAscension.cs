using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;

namespace SlaySpoon.Act4;

/// <summary>
/// The run's ascension for <see cref="Core.EndingNumbers"/>, read like vanilla monsters read theirs. Only Tough
/// Enemies (8) and up change an Ending number, so lower levels all read as 0.
/// </summary>
internal static class EndingAscension
{
	public static int Current =>
		AscensionHelper.HasAscension(AscensionLevel.DoubleBoss) ? (int)AscensionLevel.DoubleBoss
		: AscensionHelper.HasAscension(AscensionLevel.DeadlyEnemies) ? (int)AscensionLevel.DeadlyEnemies
		: AscensionHelper.HasAscension(AscensionLevel.ToughEnemies) ? (int)AscensionLevel.ToughEnemies
		: 0;
}
