using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace SlaySpoon.Characters;

/// <summary>
/// After an elite or boss win, ProgressSaveManager looks up the character's epochs (timeline unlocks): a switch over the
/// five vanilla characters that throws for any other, and EpochModel.Get(id + "2_EPOCH"), which throws for an unknown id.
/// The Watcher has no epochs, so these checks are skipped for her. Run stats and discoveries still record as usual (in
/// the modded profile only).
/// </summary>
[HarmonyPatch]
public static class WatcherProgressPatch
{
	private static bool NotWatcher(Player localPlayer) => localPlayer.Character is not Sts1Watcher;

	[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
	[HarmonyPrefix]
	public static bool Elites(Player localPlayer) => NotWatcher(localPlayer);

	[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
	[HarmonyPrefix]
	public static bool Bosses(Player localPlayer) => NotWatcher(localPlayer);

	[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
	[HarmonyPrefix]
	public static bool CharUnlock(Player localPlayer) => NotWatcher(localPlayer);
}
