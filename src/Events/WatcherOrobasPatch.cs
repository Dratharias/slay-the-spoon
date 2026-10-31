using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using SlaySpoon.Characters;
using SlaySpoon.Core;
using SlaySpoon.Relics.Watcher;

namespace SlaySpoon.Events;

/// <summary>Replace only the unavailable Tooth slot for the Watcher, using the owner's synced event RNG.</summary>
[HarmonyPatch]
internal static class WatcherOrobasPatch
{
	private static readonly AsyncLocal<Orobas?> Generating = new();
	private static readonly Func<AncientEventModel, RelicModel, string, string?, EventOption> MakeOption =
		AccessTools.MethodDelegate<Func<AncientEventModel, RelicModel, string, string?, EventOption>>(
			AccessTools.Method(typeof(AncientEventModel), "RelicOption", [typeof(RelicModel), typeof(string), typeof(string)]));

	[HarmonyPatch(typeof(Orobas), "GenerateInitialOptions")]
	[HarmonyPrefix]
	private static void Begin(Orobas __instance, out Orobas? __state)
	{
		__state = Generating.Value;
		Generating.Value = __instance;
	}

	[HarmonyPatch(typeof(Orobas), "GenerateInitialOptions")]
	[HarmonyFinalizer]
	private static void End(Orobas? __state) => Generating.Value = __state;

	[HarmonyPatch(typeof(Orobas), "OptionPool3", MethodType.Getter)]
	[HarmonyPostfix]
	private static void Pool(Orobas __instance, ref IEnumerable<EventOption> __result)
	{
		// Inspection/preloading of AllPossibleOptions must never draw from the event RNG.
		if (Generating.Value != __instance || __instance.Owner?.Character is not Sts1Watcher) return;
		var options = __result.ToList();
		// Vanilla SetupForPlayer has already run: an absent Tooth is exactly its failed condition.
		if (!WatcherOrobasRule.UseFallback(true, options.Any(o => o.Relic is ArchaicTooth))) return;
		options.RemoveAll(o => o.TextKey == "OROBAS.pages.INITIAL.options.OPTION_POOL_3_LOCKED");
		// EventModel seeds this private stream with the run seed, event id and owner slot. No other vanilla RNG is used.
		RelicModel relic = WatcherOrobasRule.HolyWater(__instance.Rng.NextInt(2))
			? ModelDb.Relic<Sts1HolyWater>().ToMutable()
			: ModelDb.Relic<Sts1VioletLotus>().ToMutable();
		options.Add(MakeOption(__instance, relic, "INITIAL", null));
		__result = options;
	}

	[HarmonyPatch(typeof(Orobas), nameof(Orobas.AllPossibleOptions), MethodType.Getter)]
	[HarmonyPostfix]
	private static void Catalog(Orobas __instance, ref IEnumerable<EventOption> __result)
	{
		__result = __result.Concat(new[]
		{
			MakeOption(__instance, ModelDb.Relic<Sts1HolyWater>().ToMutable(), "INITIAL", null),
			MakeOption(__instance, ModelDb.Relic<Sts1VioletLotus>().ToMutable(), "INITIAL", null),
		}).ToList();
	}
}
