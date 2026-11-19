using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Achilles;
using SlaySpoon.Core;

namespace SlaySpoon.GlassCannon;

/// <summary>Which played cards break Frenzy: Block cards (the Achilles test) that are neither Attacks nor Powers.</summary>
public static class GlassCannonBlock
{
	public static bool BreaksFrenzy(CardModel card) =>
		GlassCannonRule.BreaksFrenzy(card.Type == CardType.Attack, card.Type == CardType.Power, AchillesBlock.IsBlockCard(card));
}
