using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Relics;

/// <summary>
/// Puts the Spire Souvenir in the relic pool of every vanilla character (the Necrobinder and the Regent get Hybrid
/// colorless cards from it, owner 2026-11-16), so the compendium and pool lookups
/// know it. The pool caches this list on first read, during ModelDb.Init, which runs after mods are loaded.
/// </summary>
[HarmonyPatch]
public static class SouvenirPoolPatch
{
	/// <summary>The relic pools that hold the Souvenir.</summary>
	internal static readonly Type[] Pools = { typeof(IroncladRelicPool), typeof(SilentRelicPool), typeof(DefectRelicPool),
		typeof(NecrobinderRelicPool), typeof(RegentRelicPool) };

	public static IEnumerable<MethodBase> TargetMethods() =>
		Pools.Select(pool => AccessTools.Method(pool, "GenerateAllRelics"));

	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonSpireSouvenir>()).ToList();
	}
}

/// <summary>
/// Each player's grab bag is shuffled at run start on Rng.UpFront, which then generates the map. One more relic in
/// the shuffle would draw one more number and change every room of the seed, even if the relic never drops. So the
/// Souvenir sits out the vanilla shuffle and is inserted afterwards on its own Rng: Daily and Spire Souvenir
/// runs keep the vanilla bag, and other runs differ only in Common relic draws past its slot. The grab bag is saved, so the
/// slot survives a resume.
/// </summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(Player), typeof(Rng))]
public static class SouvenirGrabBagPatch
{
	private static readonly AccessTools.FieldRef<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>> Deques =
		AccessTools.FieldRefAccess<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>>("_deques");

	[ThreadStatic]
	private static bool _populating;

	public static void Prefix() => _populating = true;

	public static void Finalizer() => _populating = false;

	public static void Postfix(RelicGrabBag __instance, Player player)
	{
		RelicModel souvenir = ModelDb.Relic<SpoonSpireSouvenir>();
		if (!player.Character.RelicPool.AllRelicIds.Contains(souvenir.Id) || !SpoonSpireSouvenir.IsOffered(player.RunState))
		{
			return;
		}
		Dictionary<RelicRarity, List<RelicModel>> deques = Deques(__instance);
		if (!deques.TryGetValue(RelicRarity.Common, out List<RelicModel>? common))
		{
			common = new List<RelicModel>();
			deques[RelicRarity.Common] = common;
		}
		int slot = new Rng(player, souvenir.Id).NextInt(common.Count + 1);
		common.Insert(slot, souvenir);
		SpoonLog.Info($"Spire Souvenir: Common slot {slot} of {common.Count} for player {player.NetId}");
	}

	/// <summary>Keeps the Souvenir out of the list the vanilla shuffle sees.</summary>
	[HarmonyPatch]
	public static class HideFromShuffle
	{
		public static IEnumerable<MethodBase> TargetMethods() =>
			SouvenirPoolPatch.Pools.Select(pool => AccessTools.Method(pool, nameof(RelicPoolModel.GetUnlockedRelics)));

		public static void Postfix(ref IEnumerable<RelicModel> __result)
		{
			if (_populating)
			{
				__result = __result.Where(r => r is not SpoonSpireSouvenir).ToList();
			}
		}
	}
}
