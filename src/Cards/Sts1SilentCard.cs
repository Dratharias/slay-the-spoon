using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Cards;

/// <summary>A Silent StS1 card: our pool for logic, the vanilla Silent pool for frame and energy colours.</summary>
public abstract class Sts1SilentCard : Sts1Card
{
	protected Sts1SilentCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	public override CardPoolModel Pool => ModelDb.CardPool<Sts1SilentCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<SilentCardPool>();

	protected override string PortraitFolder => "silent";
}
