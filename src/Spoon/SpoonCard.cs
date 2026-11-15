using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Spoon;

/// <summary>A card of the Spoon mode: token rarity, own hidden pool, outside every ordinary card source.</summary>
public abstract class SpoonCard : CardModel
{
	protected SpoonCard(int cost, CardType type, TargetType target)
		: base(cost, type, CardRarity.Token, target)
	{
	}

	public override CardPoolModel Pool => ModelDb.CardPool<SpoonCardPool>();

	public override CardPoolModel VisualCardPool => Pool;
}
