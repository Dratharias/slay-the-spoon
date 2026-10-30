using Godot;
using SlaySpoon.Potions.Watcher;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Characters.Pools;

/// <summary>The Watcher's three potions, added alongside the shared pool.</summary>
public sealed class Sts1WatcherPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<PotionModel> GenerateAllPotions() => new PotionModel[] { ModelDb.Potion<Sts1BottledMiracle>(), ModelDb.Potion<Sts1StancePotion>(), ModelDb.Potion<Sts1Ambrosia>() };
}
