using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Act4;

/// <summary>
/// ActModel derives the map backgrounds, the rest site scene and the combat background (scene and layers) from the
/// protected <c>FilePathIdentifier</c> (the lowercased id). The appended acts (the Ending, the Spoon) have no such
/// assets, so every consumer that reaches the game's asset loading is pointed at Glory's.
/// </summary>
/// <remarks>
/// A postfix on the <c>FilePathIdentifier</c> getter itself did not work in game (1.3.0: "could not find directory
/// res://scenes/backgrounds/spoon_act/layers" when entering the boss): the one-line getter is inlined into its
/// callers. These patches substitute the act at the call sites instead, which inlining cannot bypass.
/// </remarks>
[HarmonyPatch]
internal static class EndingActAssetsPatch
{
	private static Glory Canon => ModelDb.Act<Glory>();

	private static readonly AccessTools.FieldRef<NMapBg, IRunState> MapRunState =
		AccessTools.FieldRefAccess<NMapBg, IRunState>("_runState");
	private static readonly AccessTools.FieldRef<NMapBg, TextureRect> MapTop =
		AccessTools.FieldRefAccess<NMapBg, TextureRect>("_mapTop");
	private static readonly AccessTools.FieldRef<NMapBg, TextureRect> MapMid =
		AccessTools.FieldRefAccess<NMapBg, TextureRect>("_mapMid");
	private static readonly AccessTools.FieldRef<NMapBg, TextureRect> MapBot =
		AccessTools.FieldRefAccess<NMapBg, TextureRect>("_mapBot");

	/// <summary>The act's preloaded assets: its own (missing) map and background paths become Glory's.</summary>
	[HarmonyPatch(typeof(ActModel), nameof(ActModel.AssetPaths), MethodType.Getter)]
	[HarmonyPostfix]
	private static IEnumerable<string> ActAssets(IEnumerable<string> __result, ActModel __instance)
	{
		if (__instance is not IAppendedAct) return __result;
		string own = __instance.Id.Entry.ToLowerInvariant();
		string glory = Canon.Id.Entry.ToLowerInvariant();
		return __result.Select(path => path.Contains("/map_bgs/") || path.Contains("/backgrounds/")
			? path.Replace(own, glory)
			: path).ToList();
	}

	[HarmonyPatch(typeof(NMapBg), "OnVisibilityChanged")]
	[HarmonyPostfix]
	private static void MapBackground(NMapBg __instance)
	{
		try
		{
			if (MapRunState(__instance)?.Act is not IAppendedAct) return;
			MapTop(__instance).Texture = Canon.MapTopBg;
			MapMid(__instance).Texture = Canon.MapMidBg;
			MapBot(__instance).Texture = Canon.MapBotBg;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"appended act map background failed: {e.Message}");
		}
	}

	/// <summary>The rest site preload; the background itself is built by EndingBackgroundPatch.Rest.</summary>
	[HarmonyPatch(typeof(PreloadManager), nameof(PreloadManager.LoadRoomRestSite))]
	[HarmonyPrefix]
	private static void RestSitePreload(ref ActModel actModel, IEnumerable<RestSiteOption> restSiteOptions)
	{
		if (actModel is IAppendedAct) actModel = Canon;
	}

	/// <summary>Combat background assets (preloaded on entering a fight; EndingBackgroundPatch.Combat draws the room).</summary>
	[HarmonyPatch(typeof(EncounterModel), "GetBackgroundAssets")]
	[HarmonyPrefix]
	private static void CombatBackground(ref ActModel parentAct, Rng rng)
	{
		if (parentAct is IAppendedAct) parentAct = Canon;
	}
}
