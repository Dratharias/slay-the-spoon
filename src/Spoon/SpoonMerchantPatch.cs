using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// Spoon mode: every merchant stocks one Fruit Juice in place of a random potion slot (none if it already sells one).
/// The replaced slot's vanilla draws already happened, so the Shops stream sits where vanilla leaves it; the Juice's
/// price variation comes from the mod's own named stream (<see cref="SpoonJuiceCostPatch"/>), never from Shops.
/// </summary>
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
		entries[slot] = SpoonJuiceCostPatch.Create(player, rng);
	}
}

/// <summary>
/// The MerchantPotionEntry constructor calls CalcCost, which draws <c>PlayerRng.Shops</c>. While the Juice entry is
/// built, this prefix sets the same price (rarity base × 0.95–1.05, rounded) from the given named Rng instead, so no
/// vanilla stream moves. Restocks after a purchase run CalcCost untouched, as in vanilla.
/// </summary>
[HarmonyPatch(typeof(MerchantPotionEntry), nameof(MerchantPotionEntry.CalcCost))]
internal static class SpoonJuiceCostPatch
{
	private static readonly AccessTools.FieldRef<MerchantEntry, int> Cost =
		AccessTools.FieldRefAccess<MerchantEntry, int>("_cost");

	private static readonly Func<PotionRarity, int> BaseCost =
		AccessTools.MethodDelegate<Func<PotionRarity, int>>(AccessTools.Method(typeof(MerchantPotionEntry), "GetCost"));

	[ThreadStatic] private static Rng? _priceRng;

	public static MerchantPotionEntry Create(Player player, Rng priceRng)
	{
		_priceRng = priceRng;
		try
		{
			return new MerchantPotionEntry(ModelDb.Potion<FruitJuice>().ToMutable(), player);
		}
		finally
		{
			_priceRng = null;
		}
	}

	[HarmonyPrefix]
	private static bool PriceFromSpoonRng(MerchantPotionEntry __instance)
	{
		if (_priceRng is not { } rng || __instance.Model == null) return true;
		Cost(__instance) = (int)Mathf.Round(BaseCost(__instance.Model.Rarity) * rng.NextFloat(0.95f, 1.05f));
		return false;
	}
}

/// <summary>Promoted event/Ancient relics use their Spoon tier instead of vanilla's prohibitive sentinel price.
/// Uses exactly one Shops draw, like a normal relic, including on restock; ordinary price hooks still apply.</summary>
[HarmonyPatch(typeof(MerchantRelicEntry), nameof(MerchantRelicEntry.CalcCost))]
internal static class SpoonRelicCostPatch
{
	private static readonly AccessTools.FieldRef<MerchantEntry, Player> PlayerOf =
		AccessTools.FieldRefAccess<MerchantEntry, Player>("_player");
	private static readonly AccessTools.FieldRef<MerchantEntry, int> Cost =
		AccessTools.FieldRefAccess<MerchantEntry, int>("_cost");

	private static bool Prefix(MerchantRelicEntry __instance)
	{
		Player player = PlayerOf(__instance);
		if (!SpoonSpoon.IsActive(player.RunState) || __instance.Model is not { } relic ||
			SpoonRule.ExtraRelicPrice(relic.Id.Entry) is not { } basePrice) return true;
		Cost(__instance) = (int)Math.Round(basePrice * player.PlayerRng.Shops.NextFloat(0.85f, 1.15f));
		return false;
	}
}
