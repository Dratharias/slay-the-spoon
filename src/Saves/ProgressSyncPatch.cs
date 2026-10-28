using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using SlaySpoon.Core;

namespace SlaySpoon.Saves;

/// <summary>
/// With mods active, StS2 keeps a separate "modded" profile that starts with nothing unlocked
/// (no characters, no Neow, so Sealed Deck gets no cards). Right before the game loads progress,
/// copy the regular profile's progress.save over the modded one. One way only: the regular save is
/// read, never written.
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.LoadProgress))]
public static class ProgressSyncPatch
{
	private static readonly AccessTools.FieldRef<ProgressSaveManager, ISaveStore> SaveStore =
		AccessTools.FieldRefAccess<ProgressSaveManager, ISaveStore>("_saveStore");

	private static readonly AccessTools.FieldRef<ProgressSaveManager, IProfileIdProvider> ProfileIdProvider =
		AccessTools.FieldRefAccess<ProgressSaveManager, IProfileIdProvider>("_profileIdProvider");

	public static void Prefix(ProgressSaveManager __instance)
	{
		if (!UserDataPathProvider.IsRunningModded)
		{
			return;
		}
		try
		{
			int profileId = ProfileIdProvider(__instance).CurrentProfileId;
			string vanillaPath = ProgressPaths.Vanilla(profileId);
			string moddedPath = ProgressPaths.Modded(profileId);
			ISaveStore store = SaveStore(__instance);
			string? vanilla = store.FileExists(vanillaPath) ? store.ReadFile(vanillaPath) : null;
			if (string.IsNullOrEmpty(vanilla))
			{
				SpoonLog.Info($"no regular progress at {vanillaPath}; modded progress left as is");
				return;
			}
			store.WriteFile(moddedPath, vanilla);
			SpoonLog.Info($"copied regular progress to {moddedPath}");
		}
		catch (Exception e)
		{
			// Never block the game's startup over this convenience.
			SpoonLog.Info($"progress sync skipped: {e.Message}");
		}
	}
}
