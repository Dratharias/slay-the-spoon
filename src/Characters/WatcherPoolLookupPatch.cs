using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Potions.Sts1;

namespace SlaySpoon.Characters;

/// <summary>
/// RelicModel.Pool and PotionModel.Pool search ModelDb.AllRelicPools / AllPotionPools, which are built from
/// ModelDb.AllCharacters and so leave out the Watcher's pools and the shared StS1 potion pool: their First() would throw for her items (energy icons in
/// tooltips read it). Answer for her pools first.
/// </summary>
public static class WatcherPoolLookupPatch
{
	[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Pool), MethodType.Getter)]
	public static class RelicPool
	{
		public static bool Prefix(RelicModel __instance, ref RelicPoolModel __result)
		{
			RelicPoolModel pool = ModelDb.RelicPool<Sts1WatcherRelicPool>();
			if (!pool.AllRelicIds.Contains(__instance.Id))
			{
				return true;
			}
			__result = pool;
			return false;
		}
	}

	[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.Pool), MethodType.Getter)]
	public static class PotionPool
	{
		public static bool Prefix(PotionModel __instance, ref PotionPoolModel __result)
		{
			foreach (PotionPoolModel pool in new PotionPoolModel[] { ModelDb.PotionPool<Sts1WatcherPotionPool>(), ModelDb.PotionPool<Sts1PotionPool>() })
			{
				if (pool.AllPotionIds.Contains(__instance.Id))
				{
					__result = pool;
					return false;
				}
			}
			return true;
		}
	}
}
