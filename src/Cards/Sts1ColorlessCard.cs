using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Core;

namespace SlaySpoon.Cards;

public abstract class Sts1ColorlessCard : Sts1Card
{
	protected Sts1ColorlessCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	public bool IsEventOnly => ColorlessCardRules.IsEventOnly(Id.Entry);
	public override CardRarity Rarity => IsEventOnly ? CardRarity.Event : base.Rarity;

	public override CardPoolModel Pool => IsEventOnly
		? ModelDb.CardPool<Sts1EventCardPool>() : ModelDb.CardPool<Sts1ColorlessCardPool>();

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();
	public override bool CanBeGeneratedInCombat => !IsEventOnly && base.CanBeGeneratedInCombat;
	protected override string PortraitFolder => "colorless";
}
