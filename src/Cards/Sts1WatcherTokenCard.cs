using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Characters.Pools;

namespace SlaySpoon.Cards;

/// <summary>Generated and choice-only cards, excluded from every ordinary card pool.</summary>
public abstract class Sts1WatcherTokenCard : Sts1Card
{
    protected Sts1WatcherTokenCard(int cost, CardType type, TargetType target)
        : base(cost, type, CardRarity.Token, target) { }
    public override CardPoolModel Pool => ModelDb.CardPool<Sts1WatcherTokenCardPool>();
    public override CardPoolModel VisualCardPool => Pool;
    protected override string PortraitFolder => "watchertokens";
}
