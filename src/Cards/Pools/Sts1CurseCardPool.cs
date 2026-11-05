using Godot;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Pools;

/// <summary>Relic-granted StS1 curses; never paired with the vanilla curse pool, so random curses stay vanilla.</summary>
public sealed class Sts1CurseCardPool : CardPoolModel
{
	public override string Title => "sts1_curse";
	public override string EnergyColorName => "colorless";
	public override string CardFrameMaterialPath => "card_frame_curse";
	public override Color DeckEntryCardColor => new("585B61FF");
	public override bool IsColorless => false;
	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1CurseCard>();
}
