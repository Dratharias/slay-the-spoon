using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Act4;

/// <summary>
/// EncounterModel loads its slot scene from a private path derived from its id. The Ending elite has no scene in
/// the PCK, so these patches build it in code and keep the missing path out of asset preloading.
/// </summary>
[HarmonyPatch]
internal static class EndingEncounterScenePatch
{
	[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateScene))]
	[HarmonyPrefix]
	private static bool CreateScene(EncounterModel __instance, ref Control __result)
	{
		if (__instance is not Sts1ShieldAndSpearElite) return true;
		__result = Sts1ShieldAndSpearElite.CreateSlots();
		return false;
	}

	[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.GetAssetPaths))]
	[HarmonyPostfix]
	private static IEnumerable<string> GetAssetPaths(IEnumerable<string> __result, EncounterModel __instance)
	{
		if (__instance is not Sts1ShieldAndSpearElite) return __result;
		string scenePath = SceneHelper.GetScenePath("encounters/" + __instance.Id.Entry.ToLowerInvariant());
		return __result.Where(path => path != scenePath).ToList();
	}
}
