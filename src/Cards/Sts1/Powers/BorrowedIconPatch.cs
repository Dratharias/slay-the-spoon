using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Powers;

[HarmonyPatch(typeof(PowerModel))]
public static class BorrowedIconPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(PowerModel.PackedIconPath), MethodType.Getter)]
	public static void PackedIcon(PowerModel __instance, ref string __result)
	{
		if (__instance is IBorrowedIcon borrowed)
		{
			__result = Source(borrowed).PackedIconPath;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(PowerModel.ResolvedBigIconPath), MethodType.Getter)]
	public static void BigIcon(PowerModel __instance, ref string __result)
	{
		if (__instance is IBorrowedIcon borrowed)
		{
			__result = Source(borrowed).ResolvedBigIconPath;
		}
	}

	private static PowerModel Source(IBorrowedIcon borrowed)
	{
		return ModelDb.GetById<PowerModel>(ModelDb.GetId(borrowed.IconSource));
	}
}
