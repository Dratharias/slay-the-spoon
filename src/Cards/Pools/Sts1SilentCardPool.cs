using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>
/// Every Silent StS1 card class. Not attached to any character: the card set mode decides when it is drawn
/// from. Visual members copy the vanilla Silent pool.
/// </summary>
public sealed class Sts1SilentCardPool : CardPoolModel
{
	public override string Title => "sts1_silent";

	public override string EnergyColorName => "silent";

	public override string CardFrameMaterialPath => "card_frame_green";

	public override Color DeckEntryCardColor => new Color("5EBD00");

	public override Color EnergyOutlineColor => new Color("1A6625");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1SilentCard>();
}
