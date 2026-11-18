using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Cards.Sts1.Curses;

namespace SlaySpoon.Curses;

/// <summary>The curses the Cursed Deck mode hands out: the game's own generable curses plus StS1 Pain, Parasite and Pride.</summary>
public static class CursedDeckCurses
{
	/// <summary>Canonical curse cards: the vanilla pool minus Ascender's Bane and Curse of the Bell (events also give the
	/// ones Cursed Run skips, such as Bad Luck and Greed), plus three StS1 curses. The Necronomicurse is never in it.</summary>
	public static IReadOnlyList<CardModel> Pool(Player player)
	{
		var list = ModelDb.CardPool<CurseCardPool>()
			.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			.Where(c => c is not (AscendersBane or CurseOfTheBell))
			.ToList();
		list.Add(ModelDb.Card<Sts1Pain>());
		list.Add(ModelDb.Card<Sts1Parasite>());
		list.Add(ModelDb.Card<Sts1Pride>());
		return list;
	}

	/// <summary>Draws one curse from <see cref="Pool"/> on the given Rng and creates it for the player (not yet in the deck).</summary>
	public static CardModel Create(Player player, Rng rng) =>
		player.RunState.CreateCard(rng.NextItem(Pool(player))!, player);

	/// <summary>Draws a curse and adds it to the player's deck with the usual preview.</summary>
	public static async Task<CardModel> AddRandom(Player player, Rng rng)
	{
		CardModel card = Create(player, rng);
		CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
		if (MegaCrit.Sts2.Core.Context.LocalContext.IsMe(player)) CardCmd.PreviewCardPileAdd(result);
		return card;
	}
}
