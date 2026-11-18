using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.CardSets;
using SlaySpoon.Core;

namespace SlaySpoon.Potions.Sts1;

/// <summary>
/// Every random potion (rewards, merchant, in-combat generation) is drawn from <see cref="PotionFactory.GetPotionOptions"/>:
/// in StS1-only runs it also lists <see cref="Sts1PotionPool"/>. Other modes, Standard and Daily stay vanilla.
/// </summary>
[HarmonyPatch(typeof(PotionFactory), nameof(PotionFactory.GetPotionOptions))]
public static class Sts1PotionOptionsPatch
{
	// Since 0.111 the callers apply the blacklist after this call, so it covers the added potions too.
	public static void Postfix(Player player, ref IEnumerable<PotionModel> __result)
	{
		if (!Sts1PotionRule.Offered(CardSetPools.ModeOf(player.RunState))) return;
		__result = __result.Concat(ModelDb.PotionPool<Sts1PotionPool>().AllPotions).ToList();
	}
}
