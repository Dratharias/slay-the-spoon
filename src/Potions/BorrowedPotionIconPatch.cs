using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Potions;

/// <summary>Patch the packed getters so the image, outline and preloads all use the borrowed potion art.</summary>
[HarmonyPatch(typeof(PotionModel))]
public static class BorrowedPotionIconPatch
{
	[HarmonyPostfix]
	[HarmonyPatch("PackedImagePath", MethodType.Getter)]
	public static void Image(PotionModel __instance, ref string __result)
	{
		if (__instance is IBorrowedIcon borrowed)
			__result = Source(borrowed).ImagePath;
	}
	[HarmonyPostfix]
	[HarmonyPatch("PackedOutlinePath", MethodType.Getter)]
	public static void Outline(PotionModel __instance, ref string __result)
	{
		if (__instance is IBorrowedIcon borrowed && Source(borrowed).OutlinePath is { } path)
			__result = path;
	}
	private static PotionModel Source(IBorrowedIcon borrowed) => ModelDb.GetById<PotionModel>(ModelDb.GetId(borrowed.IconSource));
}
