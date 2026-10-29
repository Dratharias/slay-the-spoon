using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Cards;

/// <summary>An Ironclad StS1 card: our pool for logic, the vanilla Ironclad pool for frame and energy colours.</summary>
public abstract class Sts1IroncladCard : Sts1Card
{
	protected Sts1IroncladCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	public override CardPoolModel Pool => ModelDb.CardPool<Sts1IroncladCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<IroncladCardPool>();

	protected override string PortraitFolder => "ironclad";
}
