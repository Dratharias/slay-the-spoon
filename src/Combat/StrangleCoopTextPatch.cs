using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Combat;

/// <summary>
/// While <see cref="StrangleCoopPatch"/> lets every player's cards trigger Strangle (and StS1 Choke), its hover tip says
/// so: vanilla's "Whenever you play a card" and "Whenever {Applier} plays a card" become "Whenever any player plays a
/// card" (<c>SPOON_STRANGLE_COOP.smartDescription</c>). Standard and Daily keep the vanilla text.
/// </summary>
[HarmonyPatch]
public static class StrangleCoopTextPatch
{
	private const string Key = "SPOON_STRANGLE_COOP.smartDescription";

	[HarmonyPatch(typeof(PowerModel), "SmartDescriptionLocKey", MethodType.Getter)]
	[HarmonyPostfix]
	public static void Smart(PowerModel __instance, ref string __result) => Swap(__instance, ref __result);

	[HarmonyPatch(typeof(PowerModel), "RemoteDescriptionLocKey", MethodType.Getter)]
	[HarmonyPostfix]
	public static void Remote(PowerModel __instance, ref string __result) => Swap(__instance, ref __result);

	private static void Swap(PowerModel power, ref string key)
	{
		if (power is not StranglePower || !power.IsMutable)
		{
			return;
		}
		var run = power.Owner?.CombatState?.RunState;
		if (run != null && StrangleRule.CountsAllies(run.GameMode == GameMode.Custom, run.Players.Count))
		{
			key = Key;
		}
	}
}
