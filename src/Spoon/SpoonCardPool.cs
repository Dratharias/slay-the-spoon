using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Spoon;

/// <summary>The Spoon mode's own cards; hidden, never registered in ModelDb.AllCardPools.</summary>
public sealed class SpoonCardPool : CardPoolModel
{
	public override string Title => "spoon";
	public override string EnergyColorName => "colorless";
	public override string CardFrameMaterialPath => "card_frame_colorless";
	public override Color DeckEntryCardColor => Colors.White;
	public override bool IsColorless => true;
	protected override CardModel[] GenerateAllCards() =>
	[
		ModelDb.Card<SpoonSpoonful>(), ModelDb.Card<SpoonTeaspoon>(),
		ModelDb.Card<SpoonWoodenSpoon>(), ModelDb.Card<SpoonSkimmer>(), ModelDb.Card<SpoonMelonBaller>(),
		ModelDb.Card<SpoonStolenSpoonful>(),
	];
}
