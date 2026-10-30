using Godot;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Characters.Pools;

public sealed class Sts1WatcherTokenCardPool : CardPoolModel
{
    public override string Title => "token";
    public override string EnergyColorName => "colorless";
    public override string CardFrameMaterialPath => "card_frame_colorless";
    public override Color DeckEntryCardColor => Colors.White;
    public override bool IsColorless => true;
    protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1WatcherTokenCard>();
}
