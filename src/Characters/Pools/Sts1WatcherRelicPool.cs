using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Relics.Watcher;

namespace SlaySpoon.Characters.Pools;

/// <summary>
/// The Watcher's relics. Only the starter for now; the grab bag keeps Common to Shop rarities, so Pure Water never
/// drops. Outside ModelDb.AllRelicPools: <see cref="WatcherPoolLookupPatch"/> answers RelicModel.Pool for it.
/// </summary>
public sealed class Sts1WatcherRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<RelicModel> GenerateAllRelics() => new RelicModel[] { ModelDb.Relic<Sts1PureWater>() };
}
