using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>Ordinary colorless cards only; never register this hidden pool in ModelDb.AllCardPools.</summary>
public sealed class Sts1ColorlessCardPool : CardPoolModel
{
	public override string Title => "sts1_colorless";
	public override string EnergyColorName => "colorless";
	public override string CardFrameMaterialPath => "card_frame_colorless";
	public override Color DeckEntryCardColor => new("A3A3A3FF");
	public override bool IsColorless => true;
	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1ColorlessCard>()
		.Where(c => !((Sts1ColorlessCard)c).IsEventOnly).ToArray();
}
