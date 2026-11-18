using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Core;

namespace SlaySpoon.DeckOfTen;

/// <summary>
/// Standard exception (owner, 2026-11-18): Neow's cursed option is the Deck of 10 relic 1 time in 11
/// (<see cref="DeckOfTenRule.NeowOdds"/>). Applies wherever Neow draws its vanilla options. The roll uses the mod's own
/// stream from the run seed, after vanilla drew its options, so Neow's own draws and every other seed stay vanilla.
/// </summary>
[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.Events.Neow), "GenerateInitialOptions")]
public static class DeckOfTenNeowPatch
{
	private static readonly MethodInfo RelicOption = AccessTools.Method(typeof(AncientEventModel), "RelicOption",
		new[] { typeof(RelicModel), typeof(string), typeof(string) });

	private static readonly HashSet<Type> CursedRelics = new()
	{
		typeof(CursedPearl), typeof(DowsingRod), typeof(HeftyTablet), typeof(LargeCapsule), typeof(LeafyPoultice),
		typeof(NeowsBones), typeof(NeowsSacrifice), typeof(PrecariousShears), typeof(SilkenTress), typeof(SilverCrucible),
	};

	public static void Postfix(MegaCrit.Sts2.Core.Models.Events.Neow __instance, ref IReadOnlyList<EventOption> __result)
	{
		try
		{
			int cursed = __result.Count - 1;
			if (cursed < 0 || __result[cursed].Relic is not { } relic || !CursedRelics.Contains(relic.GetType())) return;
			var player = __instance.Owner;
			var deckOfTen = ModelDb.Relic<SpoonDeckOfTenRelic>();
			if (player == null || player.Relics.Any(r => r.Id == deckOfTen.Id)) return;
			var run = player.RunState;
			var rng = new Rng(run.Rng.Seed, DeckOfTenRule.NeowStream(run.GetPlayerSlotIndex(player)));
			if (!DeckOfTenRule.OfferedAtNeow(rng.NextInt(DeckOfTenRule.NeowOdds))) return;
			var option = (EventOption)RelicOption.Invoke(__instance,
				new object?[] { deckOfTen.ToMutable(), "INITIAL", "NEOW.pages.DONE.CURSED.description" })!;
			var options = __result.ToList();
			options[cursed] = option;
			__result = options;
			SpoonLog.Info($"deck of 10: Neow offers it to {player.NetId} instead of {relic.Id.Entry}");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"deck of 10: Neow option failed: {e.Message}");
		}
	}
}
