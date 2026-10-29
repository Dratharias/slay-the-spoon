using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Card set mode for the run. One sealed class per mode (none ticked = StS2 only), so the mode travels as the
/// modifier id like the percents. The pools are swapped by <see cref="CardSets.UnlockedCardsPatch"/>.
/// </summary>
public abstract class SpoonCardSet : ModifierModel
{
	public abstract CardSetMode Mode { get; }
}

public sealed class SpoonCardSetSts1Only : SpoonCardSet
{
	public override CardSetMode Mode => CardSetMode.Sts1Only;
}

public sealed class SpoonCardSetHybrid : SpoonCardSet
{
	public override CardSetMode Mode => CardSetMode.Hybrid;
}
