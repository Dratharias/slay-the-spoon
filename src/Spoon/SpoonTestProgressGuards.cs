using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using SlaySpoon.Config;

namespace SlaySpoon.Spoon;

/// <summary>
/// A "Test: The Spoon" run (<see cref="SpoonTestRun"/>) leaves the modded profile's progress alone. As a Custom Run it
/// already earns no epochs, achievements, wins or streaks; these guards also skip what Custom Runs still record.
/// The run history entry stays (the game over screen reads it); it shows as a Custom Run in the modded profile.
/// </summary>
public static class SpoonTestProgressGuards
{
	/// <summary>Run end: play time, floors climbed, acts seen, encounter and event stats.</summary>
	[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.UpdateWithRunData))]
	internal static class RunEndPatch
	{
		private static bool Prefix(SerializableRun serializableRun)
		{
			if (!SpoonTestRun.IsActive(serializableRun)) return true;
			SpoonLog.Info("spoon test: run end progress skipped");
			return false;
		}
	}

	/// <summary>Each combat won: encounter and enemy win stats.</summary>
	[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.UpdateAfterCombatWon))]
	internal static class CombatWonPatch
	{
		private static bool Prefix(Player localPlayer) => !SpoonTestRun.IsActive(localPlayer?.RunState);
	}

	/// <summary>Game over screen: the score bar that unlocks timeline epochs, even after a Custom Run.</summary>
	[HarmonyPatch(typeof(NGameOverScreen), "AnimateScoreBar")]
	internal static class ScoreBarPatch
	{
		private static readonly AccessTools.FieldRef<NGameOverScreen, SerializableRun> Run =
			AccessTools.FieldRefAccess<NGameOverScreen, SerializableRun>("_serializableRun");

		private static bool Prefix(NGameOverScreen __instance, ref Task __result)
		{
			if (!SpoonTestRun.IsActive(Run(__instance))) return true;
			SpoonLog.Info("spoon test: unlock score skipped");
			__result = Task.CompletedTask;
			return false;
		}
	}
}
