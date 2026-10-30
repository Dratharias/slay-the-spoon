using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Relics.Watcher;

namespace SlaySpoon.Characters.Pools;

/// <summary>Watcher-specific relics. Native Cloak Clasp remains in the shared pool.</summary>
public sealed class Sts1WatcherRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<RelicModel> GenerateAllRelics() => new RelicModel[] { ModelDb.Relic<Sts1PureWater>(), ModelDb.Relic<Sts1HolyWater>(), ModelDb.Relic<Sts1VioletLotus>(),
        ModelDb.Relic<Sts1Damaru>(), ModelDb.Relic<Sts1TeardropLocket>(), ModelDb.Relic<Sts1Melange>(),
        ModelDb.Relic<Sts1GoldenEye>(), ModelDb.Relic<Sts1Duality>() };
}
