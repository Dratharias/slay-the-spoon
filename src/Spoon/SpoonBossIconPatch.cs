using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Spoon;

/// <summary>
/// The top bar boss and room icons and the run history build a boss's icon path from its encounter id, which the
/// Spoon doesn't have (as for the Heart, <c>EndingBossIconPatch</c>). Point them at its map node icon.
/// </summary>
[HarmonyPatch]
internal static class SpoonBossIconPatch
{
	private static readonly ModelId SpoonBossId = ModelDb.GetId(typeof(SpoonBoss));

	[HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconPath))]
	[HarmonyPostfix]
	private static void Icon(ModelId? modelId, ref string? __result)
	{
		if (IsSpoon(modelId)) __result = SpoonPath(".png");
	}

	[HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconOutlinePath))]
	[HarmonyPostfix]
	private static void Outline(ModelId? modelId, ref string? __result)
	{
		if (IsSpoon(modelId)) __result = SpoonPath("_outline.png");
	}

	private static bool IsSpoon(ModelId? modelId) => modelId != null && modelId == SpoonBossId;

	private static string SpoonPath(string suffix) => ModelDb.Encounter<SpoonBoss>().BossNodePath + suffix;
}
