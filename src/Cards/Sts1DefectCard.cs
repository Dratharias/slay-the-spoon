using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Cards;

/// <summary>A Defect StS1 card: our pool for logic, the vanilla Defect pool for frame and energy colours.</summary>
public abstract class Sts1DefectCard : Sts1Card
{
	protected Sts1DefectCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	public override CardPoolModel Pool => ModelDb.CardPool<Sts1DefectCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<DefectCardPool>();

	protected override string PortraitFolder => "defect";
}
