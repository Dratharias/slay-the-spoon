using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>Event and console access only. This pool is deliberately not paired with EventCardPool.</summary>
public sealed class Sts1EventCardPool : CardPoolModel
{
	public override string Title => "sts1_event";
	public override string EnergyColorName => "colorless";
	public override string CardFrameMaterialPath => "card_frame_colorless";
	public override Color DeckEntryCardColor => new("A3A3A3FF");
	public override bool IsColorless => true;
	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1ColorlessCard>()
		.Where(c => ((Sts1ColorlessCard)c).IsEventOnly).ToArray();
}
