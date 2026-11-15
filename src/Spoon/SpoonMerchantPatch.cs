using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>Spoon mode: every merchant stocks one Fruit Juice in place of a random potion slot (none if it already sells one).</summary>
[HarmonyPatch(typeof(MerchantInventory), nameof(MerchantInventory.CreateForNormalMerchant))]
internal static class SpoonMerchantPatch
{
	private static readonly AccessTools.FieldRef<MerchantInventory, List<MerchantPotionEntry>> PotionEntries =
		AccessTools.FieldRefAccess<MerchantInventory, List<MerchantPotionEntry>>("_potionEntries");

	private static void Postfix(Player player, MerchantInventory __result)
	{
		var run = player.RunState;
		if (!SpoonSpoon.IsActive(run)) return;
		var entries = PotionEntries(__result);
		var rng = new Rng(run.Rng.Seed, $"SlaySpoon.Spoon.Juice_{run.TotalFloor}_{player.NetId}");
		int slot = SpoonRule.JuiceSlot(entries.Count, entries.Any(e => e.Model is FruitJuice), rng.NextInt(1000));
		if (slot < 0) return;
		entries[slot] = new MerchantPotionEntry(ModelDb.Potion<FruitJuice>().ToMutable(), player);
	}
}
