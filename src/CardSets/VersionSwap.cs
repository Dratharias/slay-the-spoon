using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>Turns a duplicate card into its other version (players in Hybrid only).</summary>
public static class VersionSwap
{
	/// <summary>The other version of <paramref name="card"/> when its owner is in Hybrid, else null.</summary>
	public static CardModel? PartnerOf(CardModel card)
	{
		return IsHybrid(card.Owner) ? CardSetPools.PartnerOf(card) : null;
	}

	public static bool IsHybrid(Player? player)
	{
		return CardSetPools.ModeFor(player) == CardSetMode.Hybrid;
	}

	/// <summary>
	/// A new card of the other version, created in the same scope for the same owner, at the same upgrade level.
	/// The original card is left as is (like the cards a reroll discards).
	/// </summary>
	public static CardModel Swap(CardModel card, CardModel partner)
	{
		CardModel swapped = card.CardScope!.CreateCard(partner, card.Owner);
		for (int level = 0; level < card.CurrentUpgradeLevel && swapped.IsUpgradable; level++)
		{
			swapped.UpgradeInternal();
			swapped.FinalizeUpgradeInternal();
		}
		return swapped;
	}

	/// <summary>Coin flip on the run's Niche RNG, which every co-op client advances the same way.</summary>
	public static CardModel RollVersion(CardModel card)
	{
		CardModel? partner = PartnerOf(card);
		if (partner == null || !card.RunState!.Rng.Niche.NextBool())
		{
			return card;
		}
		return Swap(card, partner);
	}

	public static CardCreationResult RollVersion(CardCreationResult result)
	{
		CardModel rolled = RollVersion(result.Card);
		return rolled == result.Card ? result : new CardCreationResult(rolled);
	}
}
