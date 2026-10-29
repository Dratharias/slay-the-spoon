using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>
/// Every Defect StS1 card class. Not attached to any character: the card set mode decides when it is drawn
/// from. Visual members copy the vanilla Defect pool.
/// </summary>
public sealed class Sts1DefectCardPool : CardPoolModel
{
	public override string Title => "sts1_defect";

	public override string EnergyColorName => "defect";

	public override string CardFrameMaterialPath => "card_frame_blue";

	public override Color DeckEntryCardColor => new Color("3EB3ED");

	public override Color EnergyOutlineColor => new Color("1D5673");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1DefectCard>();
}
