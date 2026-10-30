using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace SlaySpoon.Act4;

/// <summary>
/// Shuffles generated cards into a draw pile with an Ending monster's own RNG. CardPilePosition.Random would draw
/// from the vanilla shuffle RNG and shift the run's seeds.
/// </summary>
public static class EndingDrawPile
{
	public static async Task ShuffleIn(Player player, IEnumerable<CardModel> cards, Rng rng)
	{
		var list = cards.ToList();
		// No creator: MonsterDamageRule treats these as enemy statuses.
		await CardPileCmd.AddGeneratedCardsToCombat(list, PileType.Draw, creator: null, CardPilePosition.Top);
		var pile = PileType.Draw.GetPile(player);
		foreach (var card in list)
		{
			if (!pile.Cards.Contains(card)) continue;
			pile.RemoveInternal(card, silent: true);
			pile.AddInternal(card, rng.NextInt(pile.Cards.Count + 1), silent: true);
		}
	}
}
