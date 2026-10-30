using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Relics.Watcher;

/// <summary>
/// StS1 Pure Water, the Watcher's starter relic. Placeholder: Miracle is not ported yet, so the relic does nothing
/// until step 7, task 6 (its text already states the StS1 effect).
/// </summary>
public sealed class Sts1PureWater : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	/// <summary>The mod ships no relic art: the icons come from Ripple Basin (a basin of still water).</summary>
	protected override string IconBaseName => "ripple_basin";
}
