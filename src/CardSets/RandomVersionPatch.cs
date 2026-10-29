using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.CardSets;

/// <summary>
/// In Hybrid, a duplicate is one slot held by its StS2 card; every card the factory creates (rewards, merchant,
/// combat generation, transforms) comes out as either version at random. Reward screens then let the player swap.
/// </summary>
[HarmonyPatch(typeof(CardFactory))]
public static class RandomVersionPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.CreateForReward), typeof(Player), typeof(int), typeof(CardCreationOptions))]
	public static void Reward(Player player, ref IEnumerable<CardCreationResult> __result)
	{
		if (VersionSwap.IsHybrid(player))
		{
			__result = __result.Select(VersionSwap.RollVersion).ToList();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.CreateForMerchant), typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType))]
	public static void MerchantByType(ref CardCreationResult __result) => __result = VersionSwap.RollVersion(__result);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.CreateForMerchant), typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardRarity))]
	public static void MerchantByRarity(ref CardCreationResult __result) => __result = VersionSwap.RollVersion(__result);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.GetDistinctForCombat))]
	public static void DistinctForCombat(Player player, ref IEnumerable<CardModel> __result) => Combat(player, ref __result);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.GetForCombat))]
	public static void ForCombat(Player player, ref IEnumerable<CardModel> __result) => Combat(player, ref __result);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.CreateRandomCardForTransform), typeof(CardModel), typeof(bool), typeof(Rng))]
	public static void Transform(ref CardModel __result) => __result = VersionSwap.RollVersion(__result);

	[HarmonyPostfix]
	[HarmonyPatch(nameof(CardFactory.CreateRandomCardForTransform), typeof(CardModel), typeof(IEnumerable<CardModel>), typeof(bool), typeof(Rng))]
	public static void TransformFrom(ref CardModel __result) => __result = VersionSwap.RollVersion(__result);

	private static void Combat(Player player, ref IEnumerable<CardModel> result)
	{
		// The vanilla result may be lazy; it is enumerated once here, in the same order as vanilla would.
		if (VersionSwap.IsHybrid(player))
		{
			result = result.Select(VersionSwap.RollVersion).ToList();
		}
	}
}
