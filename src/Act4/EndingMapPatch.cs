using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Act4;

/// <summary>
/// Fresh generation of an appended act's map (the Ending, the Spoon) builds its fixed map
/// (<see cref="IAppendedAct.CreateFixedMap"/>) instead of a <c>StandardActMap</c>, whose room counts and point types
/// the act cannot fill (no events, normal fights or ancient).
/// </summary>
/// <remarks>
/// Vanilla <c>StandardActMap.CreateFor</c> draws from its own named Rng, so skipping it leaves every seed vanilla.
/// Saved maps never reach this method: <c>RunManager.GenerateMap</c> rebuilds them as a <c>SavedActMap</c>.
/// </remarks>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.CreateMap))]
internal static class EndingCreateMapPatch
{
	private static bool Prefix(ActModel __instance, ref ActMap __result)
	{
		if (__instance is not IAppendedAct act) return true;
		__result = act.CreateFixedMap();
		return false;
	}
}

/// <summary>
/// After every map listener (early and late) has run on a freshly generated map, an appended act keeps its fixed map:
/// Golden Compass, bought at the act 4 merchant, would otherwise swap in a golden path full of monsters and events,
/// which the act cannot fill. A postfix wins whatever the listener order.
/// </summary>
/// <remarks>
/// Only fresh generation calls this hook. A saved map goes through <c>Hook.ModifyGeneratedMapLate</c> alone and keeps
/// its saved topology, as vanilla intends; an appended act's map saves and reloads with the same points and links.
/// </remarks>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyGeneratedMap))]
internal static class EndingGeneratedMapPatch
{
	private static void Postfix(IRunState runState, int actIndex, ref ActMap __result)
	{
		if (actIndex < 0 || actIndex >= runState.Acts.Count || runState.Acts[actIndex] is not IAppendedAct act) return;
		if (__result.GetType() == act.FixedMapType) return;
		ActMap map = act.CreateFixedMap();
		SpoonLog.Info($"{act.LogName}: {__result.GetType().Name} replaced by {map.GetType().Name}");
		__result = map;
	}
}
