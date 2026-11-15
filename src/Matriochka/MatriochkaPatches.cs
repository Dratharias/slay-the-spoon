using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;

namespace SlaySpoon.Matriochka;

/// <summary>Matriochka's Doll Room: its only option takes Bing Bong.</summary>
[HarmonyPatch(typeof(DollRoom), "GenerateInitialOptions")]
public static class DollRoomBingBongPatch
{
	private static readonly FieldInfo Dolls = AccessTools.Field(typeof(DollRoom), "_dolls");
	private static readonly MethodInfo OptionFromChoice = AccessTools.Method(typeof(DollRoom), "OptionFromChoice");

	public static void Postfix(DollRoom __instance, ref IReadOnlyList<EventOption> __result)
	{
		if (SpoonMatriochka.Of(__instance.Owner?.RunState) == null)
			return;
		foreach (object choice in (Array)Dolls.GetValue(null)!)
		{
			var relic = (RelicModel)AccessTools.Field(choice.GetType(), "relic").GetValue(choice)!;
			if (relic is BingBong)
			{
				__result = [(EventOption)OptionFromChoice.Invoke(__instance, [choice])!];
				return;
			}
		}
		SpoonLog.Error("matriochka: Bing Bong not found in the Doll Room");
	}
}

/// <summary>Matriochka's Reflections: Shatter is the only option.</summary>
[HarmonyPatch(typeof(Reflections), "GenerateInitialOptions")]
public static class ReflectionsShatterPatch
{
	public static void Postfix(Reflections __instance, ref IReadOnlyList<EventOption> __result)
	{
		if (SpoonMatriochka.Of(__instance.Owner?.RunState) == null)
			return;
		var shatter = __result.Where(o => o.TextKey.EndsWith(".SHATTER", StringComparison.Ordinal)).ToList();
		if (shatter.Count == 1)
			__result = shatter;
		else
			SpoonLog.Error("matriochka: Shatter option not found in Reflections");
	}
}

/// <summary>Matriochka: the rewards screen cannot be left while an unskippable card reward is still unclaimed.</summary>
[HarmonyPatch(typeof(NRewardsScreen), "TryEnableProceedButton")]
public static class MandatoryCardProceedPatch
{
	private static readonly AccessTools.FieldRef<NRewardsScreen, List<Control>> Buttons =
		AccessTools.FieldRefAccess<NRewardsScreen, List<Control>>("_rewardButtons");
	private static readonly AccessTools.FieldRef<NRewardsScreen, IRunState> Run =
		AccessTools.FieldRefAccess<NRewardsScreen, IRunState>("_runState");
	private static readonly AccessTools.FieldRef<NRewardsScreen, NProceedButton> Proceed =
		AccessTools.FieldRefAccess<NRewardsScreen, NProceedButton>("_proceedButton");

	public static bool Prefix(NRewardsScreen __instance)
	{
		if (SpoonMatriochka.Of(Run(__instance)) == null)
			return true;
		bool pending = Buttons(__instance).OfType<NRewardButton>()
			.Any(b => b.Reward is CardReward { CanSkip: false, IsPopulated: true });
		if (!pending)
			return true;
		Proceed(__instance).Disable();
		return false;
	}
}
