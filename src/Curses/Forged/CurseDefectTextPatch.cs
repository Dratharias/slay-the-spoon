using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Curses.Forged;

/// <summary>A forged curse's defects, one purple line each, under its text (like the Forge's orange "Forged" line).</summary>
[HarmonyPatch]
public static class CurseDefectTextPatch
{
	public static System.Reflection.MethodBase TargetMethod() =>
		AccessTools.GetDeclaredMethods(typeof(CardModel)).First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

	public static void Postfix(CardModel __instance, ref string __result)
	{
		try
		{
			var defects = CurseDefects.Get(__instance);
			if (defects == CurseDefect.None || !ForgedCurses.Applies(__instance)) return;
			foreach (var d in ForgedCurseRule.All)
				if (defects.HasFlag(d))
					__result += "\n[purple]" + new LocString("cards", "SPOON_CURSE_DEFECT." + d).GetFormattedText() + "[/purple]";
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forged curses: defect text of {__instance.Id.Entry} failed: {e.Message}");
		}
	}
}
