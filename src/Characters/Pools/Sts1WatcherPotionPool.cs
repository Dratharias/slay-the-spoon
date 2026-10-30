using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Characters.Pools;

/// <summary>The Watcher's potions. Empty for now: potion drops come from the shared pool alone.</summary>
public sealed class Sts1WatcherPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<PotionModel> GenerateAllPotions() => Array.Empty<PotionModel>();
}
