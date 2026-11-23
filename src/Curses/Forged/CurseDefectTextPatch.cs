using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Curses.Forged;

/// <summary>
/// A forged curse's defects: one purple line of names under its text (like the Forge's orange "Forged" line), and one
/// hover tip per defect with its effect. Full sentences on the card overflowed the text box at +9 (Retain, Innate,
/// Eternal, the effect and three defects).
/// </summary>
[HarmonyPatch]
public static class CurseDefectTextPatch
{
	public static System.Reflection.MethodBase TargetMethod() =>
		AccessTools.GetDeclaredMethods(typeof(CardModel)).First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

	public static void Postfix(CardModel __instance, ref string __result)
	{
		try
		{
			var names = Defects(__instance).Select(d => Loc(d + ".name").GetFormattedText()).ToList();
			if (names.Count > 0) __result += "\n[purple]" + string.Join(" · ", names) + "[/purple]";
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forged curses: defect text of {__instance.Id.Entry} failed: {e.Message}");
		}
	}

	internal static IEnumerable<CurseDefect> Defects(CardModel card)
	{
		var defects = CurseDefects.Get(card);
		if (defects == CurseDefect.None || !ForgedCurses.Applies(card)) return [];
		return ForgedCurseRule.All.Where(d => defects.HasFlag(d));
	}

	internal static LocString Loc(string key) => new("cards", "SPOON_CURSE_DEFECT." + key);
}

/// <summary>One hover tip per defect of a forged curse: its name and effect.</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.HoverTips), MethodType.Getter)]
public static class CurseDefectHoverTipPatch
{
	public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
	{
		try
		{
			var tips = CurseDefectTextPatch.Defects(__instance)
				.Select(d => (IHoverTip)new HoverTip(CurseDefectTextPatch.Loc(d + ".name"), CurseDefectTextPatch.Loc(d.ToString())))
				.ToList();
			if (tips.Count > 0) __result = __result.Concat(tips).ToList();
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forged curses: defect tips of {__instance.Id.Entry} failed: {e.Message}");
		}
	}
}
