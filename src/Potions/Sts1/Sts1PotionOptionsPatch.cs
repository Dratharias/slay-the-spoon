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
	public static void Postfix(Player player, IEnumerable<PotionModel> blacklist, ref IEnumerable<PotionModel> __result)
	{
		if (!Sts1PotionRule.Offered(CardSetPools.ModeOf(player.RunState))) return;
		var extra = ModelDb.PotionPool<Sts1PotionPool>().AllPotions.Where(p => !blacklist.Contains(p));
		__result = __result.Concat(extra).ToList();
	}
}
