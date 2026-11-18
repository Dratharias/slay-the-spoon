using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>Appends a forged card's per-level extras (<see cref="ForgeBonuses"/>) to its text: "Forged: +4 Block, Tracking +50."</summary>
[HarmonyPatch]
public static class ForgeBonusTextPatch
{
	public static System.Reflection.MethodBase TargetMethod() =>
		AccessTools.GetDeclaredMethods(typeof(CardModel)).First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

	private static readonly Dictionary<string, string> PowerIds = new(StringComparer.Ordinal)
	{
		["Strength"] = "STRENGTH_POWER", ["Dexterity"] = "DEXTERITY_POWER", ["Vigor"] = "VIGOR_POWER",
		["Poison"] = "POISON_POWER", ["Weak"] = "WEAK_POWER", ["Vulnerable"] = "VULNERABLE_POWER",
	};

	private static Dictionary<string, string>? _powerIdsByClass;

	public static void Postfix(CardModel __instance, ref string __result)
	{
		try
		{
			if (Line(__instance) is { } line) __result += "\n[orange]" + line + "[/orange]";
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forge: bonus text of {__instance.Id.Entry} failed: {e.Message}");
		}
	}

	private static string? Line(CardModel card)
	{
		if (card.CurrentUpgradeLevel <= 1 || ForgeCurves.For(card.Id.Entry) is not { } row) return null;
		var parts = new List<string>();
		foreach (var (kind, per) in row.Play ?? new Dictionary<string, decimal>())
		{
			decimal n = ForgeRule.PerLevelBonus(per, card.CurrentUpgradeLevel, ForgeLevels.Cap(card));
			if (n <= 0) continue;
			var name = PowerIds.TryGetValue(kind, out var pid) ? Title(pid) : new LocString("cards", "SPOON_FORGE_BONUS." + kind).GetFormattedText();
			parts.Add($"+{n:0.##} {name}");
		}
		foreach (var (power, per) in row.PowerBonus ?? new Dictionary<string, decimal>())
		{
			decimal n = ForgeRule.PerLevelBonus(per, card.CurrentUpgradeLevel, ForgeLevels.Cap(card));
			if (n <= 0 || PowerIdOf(power) is not { } pid) continue;
			parts.Add($"{Title(pid)} +{n:0.##}");
		}
		if (parts.Count == 0) return null;
		var text = new LocString("cards", "SPOON_FORGE_BONUS");
		text.Add("Bonuses", string.Join(", ", parts));
		return text.GetFormattedText();
	}

	private static string Title(string powerId) => new LocString("powers", powerId + ".title").GetFormattedText();

	private static string? PowerIdOf(string className)
	{
		_powerIdsByClass ??= ModelDb.AllPowers.ToDictionary(p => p.GetType().Name, p => p.Id.Entry, StringComparer.Ordinal);
		return _powerIdsByClass.TryGetValue(className, out var id) ? id : null;
	}
}
