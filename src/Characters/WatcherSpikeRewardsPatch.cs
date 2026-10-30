using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Unlocks;
using SlaySpoon.Characters.Pools;

namespace SlaySpoon.Characters;

/// <summary>
/// Temporary, until her card pool is ported (Task 7): her pool holds only Basic cards, which rewards, shops and
/// random generation never offer, so those would find nothing. While that is so, they draw from the Ironclad's pool.
/// </summary>
[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
public static class WatcherSpikeRewardsPatch
{
	public static void Postfix(CardPoolModel __instance, UnlockState unlockState,
		CardMultiplayerConstraint multiplayerConstraint, ref IEnumerable<CardModel> __result)
	{
		if (__instance is not Sts1WatcherCardPool || __instance.AllCards.Any(c => c.Rarity != CardRarity.Basic))
		{
			return;
		}
		__result = ModelDb.CardPool<IroncladCardPool>().GetUnlockedCards(unlockState, multiplayerConstraint);
	}
}
