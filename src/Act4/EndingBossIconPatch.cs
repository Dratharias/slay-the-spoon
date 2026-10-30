using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Act4;

/// <summary>
/// The top bar boss and room icons and the run history build a boss's icon path from its encounter id
/// (<c>res://images/ui/run_history/&lt;id&gt;.png</c> and <c>_outline.png</c>), which the Heart doesn't have.
/// Point them at the icon its map node already uses (<see cref="Sts1CorruptHeartBoss.BossNodePath"/>).
/// </summary>
[HarmonyPatch]
internal static class EndingBossIconPatch
{
	private static readonly ModelId HeartBossId = ModelDb.GetId(typeof(Sts1CorruptHeartBoss));

	[HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconPath))]
	[HarmonyPostfix]
	private static void Icon(ModelId? modelId, ref string? __result)
	{
		if (IsHeart(modelId)) __result = HeartPath(".png");
	}

	[HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconOutlinePath))]
	[HarmonyPostfix]
	private static void Outline(ModelId? modelId, ref string? __result)
	{
		if (IsHeart(modelId)) __result = HeartPath("_outline.png");
	}

	private static bool IsHeart(ModelId? modelId) => modelId != null && modelId == HeartBossId;

	private static string HeartPath(string suffix) => ModelDb.Encounter<Sts1CorruptHeartBoss>().BossNodePath + suffix;
}
