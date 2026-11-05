using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Relics.Sts1;

[HarmonyPatch]
public static class Sts1RelicPools
{
	internal static readonly Type[] Pools = { typeof(IroncladRelicPool), typeof(SilentRelicPool) };
	public static IEnumerable<MethodBase> TargetMethods() => Pools.Select(t => AccessTools.Method(t, "GenerateAllRelics"));
	public static void Postfix(RelicPoolModel __instance, ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Concat(Additions(__instance)).ToList();
	}
	internal static IEnumerable<RelicModel> Additions(RelicPoolModel pool) => pool switch
	{
		IroncladRelicPool => new RelicModel[] { ModelDb.Relic<Sts1ChampionBelt>(), ModelDb.Relic<Sts1MagicFlower>() },
		SilentRelicPool => new RelicModel[] { ModelDb.Relic<Sts1TheSpecimen>() },
		_ => Array.Empty<RelicModel>()
	};
}

[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class Sts1BossRelicPool
{
	public static void Postfix(ref IEnumerable<RelicModel> __result) => __result = __result.Concat(new RelicModel[]
	{
		ModelDb.Relic<Sts1MarkOfPain>(), ModelDb.Relic<Sts1RunicCube>(), ModelDb.Relic<Sts1HoveringKite>(),
		ModelDb.Relic<Sts1RingOfTheSerpent>(), ModelDb.Relic<Sts1WristBlade>(), ModelDb.Relic<Sts1FrozenCore>(),
		ModelDb.Relic<Sts1Inserter>(), ModelDb.Relic<Sts1NuclearBattery>()
	}).ToList();
}

/// <summary>Insert on a separate deterministic RNG after vanilla shuffling, preserving vanilla map seeds.</summary>
[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(Player), typeof(Rng))]
public static class Sts1RelicGrabBag
{
	private static readonly AccessTools.FieldRef<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>> Deques =
		AccessTools.FieldRefAccess<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>>("_deques");
	private static readonly AccessTools.FieldRef<RelicGrabBag, List<RelicModel>?> Originals =
		AccessTools.FieldRefAccess<RelicGrabBag, List<RelicModel>?>("_originalRelics");
	[ThreadStatic] private static bool _populating;
	public static void Prefix() => _populating = true;
	public static void Finalizer() => _populating = false;
	public static void Postfix(RelicGrabBag __instance, Player player)
	{
		if (!Sts1Relic.Offered(player.RunState)) return;
		foreach (var relic in Sts1RelicPools.Additions(player.Character.RelicPool))
		{
			var deques = Deques(__instance);
			if (!deques.TryGetValue(relic.Rarity, out var list)) deques[relic.Rarity] = list = new();
			list.Insert(new Rng(player, relic.Id).NextInt(list.Count + 1), relic);
			Originals(__instance)?.Add(relic);
		}
	}
	[HarmonyPatch]
	public static class HideFromVanillaShuffle
	{
		public static IEnumerable<MethodBase> TargetMethods() => Sts1RelicPools.Pools.Select(t => AccessTools.Method(t, nameof(RelicPoolModel.GetUnlockedRelics)));
		public static void Postfix(ref IEnumerable<RelicModel> __result)
		{
			if (_populating) __result = __result.Where(r => r is not Sts1Relic).ToList();
		}
	}
}
