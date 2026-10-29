using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>
/// Every Ironclad StS1 card class. Not attached to any character: the card set mode decides when it is drawn
/// from. Visual members copy the vanilla Ironclad pool.
/// </summary>
public sealed class Sts1IroncladCardPool : CardPoolModel
{
	public override string Title => "sts1_ironclad";

	public override string EnergyColorName => "ironclad";

	public override string CardFrameMaterialPath => "card_frame_red";

	public override Color DeckEntryCardColor => new Color("D62000");

	public override Color EnergyOutlineColor => new Color("802020");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1IroncladCard>();
}
