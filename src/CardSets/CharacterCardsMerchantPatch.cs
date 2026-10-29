using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Vanilla "&lt;Character&gt; Cards" skips the merchant's character offer as soon as one card isn't from the player's
/// own pool, which every StS1 card fails. When StS1 cards are offered, the offer is judged by
/// <see cref="MerchantPoolRule"/> and the other character's cards are added the vanilla way. Offers without StS1
/// cards run vanilla untouched.
/// </summary>
[HarmonyPatch(typeof(CharacterCards), nameof(CharacterCards.ModifyMerchantCardPool))]
public static class CharacterCardsMerchantPatch
{
	/// <summary>The offer is materialized once, so a lazy caller isn't enumerated twice when vanilla runs.</summary>
	public static bool Prefix(CharacterCards __instance, Player player, ref IEnumerable<CardModel> options,
		ref IEnumerable<CardModel> __result)
	{
		CardModel[] offer = options.ToArray();
		options = offer;
		CardPoolModel own = player.Character.CardPool;
		CardPoolModel? sts1 = CardSetPools.Sts1PoolOf(own);
		if (sts1 == null || !offer.Any(c => c.Pool == sts1)
			|| !MerchantPoolRule.IsCharacterOffer(offer.Select(c => c.Pool), own, sts1))
		{
			return true;
		}
		__result = offer.Concat(ModelDb.GetById<CharacterModel>(__instance.CharacterModel).CardPool
			.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint));
		return false;
	}
}
