using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Potions.Sts1;

/// <summary>
/// StS1 potions shared by every character. Like the Watcher's pool it stays out of ModelDb.AllPotionPools (built from
/// the vanilla characters), so Standard and Daily never see it; <see cref="Sts1PotionOptionsPatch"/> offers it.
/// </summary>
public sealed class Sts1PotionPool : PotionPoolModel
{
	public override string EnergyColorName => "colorless";

	public override Color LabOutlineColor => StsColors.cream;

	protected override IEnumerable<PotionModel> GenerateAllPotions() => new PotionModel[] { ModelDb.Potion<Sts1AncientPotion>() };
}
