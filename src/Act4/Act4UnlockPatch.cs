using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using SlaySpoon.Core;
using SlaySpoon.Saves;

namespace SlaySpoon.Act4;

/// <summary>
/// Records the permanent Act 4 unlock in the mod config (never in the game's saves): the first time a Standard run at
/// Ascension 10 or higher is in Act 3. Fresh runs get there through SetActInternal; a resumed save sets its act
/// index while loading and never calls it, so InitializeSavedRun is checked too.
/// </summary>
public static class Act4UnlockPatch
{
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	private static readonly AccessTools.FieldRef<ProgressSaveManager, IProfileIdProvider> ProfileIdProvider =
		AccessTools.FieldRefAccess<ProgressSaveManager, IProfileIdProvider>("_profileIdProvider");

	/// <summary>The profile id a progress save manager works with.</summary>
	public static int CurrentProfileId(ProgressSaveManager manager) => ProfileIdProvider(manager).CurrentProfileId;

	/// <summary>The active profile id (SaveManager is the game's profile id provider).</summary>
	public static int CurrentProfileId() => SaveManager.Instance.CurrentProfileId;

	public static void Record()
	{
		try
		{
			RunState? run = State(RunManager.Instance);
			if (run == null || !Act4UnlockRule.ShouldUnlock(run.GameMode == GameMode.Standard, run.AscensionLevel, run.CurrentActIndex))
			{
				return;
			}
			int profileId = CurrentProfileId();
			if (SpoonConfigFile.Act4Unlocked(profileId))
			{
				return;
			}
			SpoonConfigFile.SaveAct4Unlocked(profileId);
			SpoonLog.Info($"act4: unlocked for profile {profileId}");
		}
		catch (Exception e)
		{
			// Never break a run over a convenience record.
			SpoonLog.Error($"act4: unlock check failed: {e}");
		}
	}
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetActInternal))]
public static class Act4UnlockActPatch
{
	public static void Postfix(ref Task __result) => __result = Then(__result);

	private static async Task Then(Task original)
	{
		await original;
		Act4UnlockPatch.Record();
	}
}

[HarmonyPatch(typeof(RunManager), "InitializeSavedRun")]
public static class Act4UnlockSavedRunPatch
{
	public static void Postfix() => Act4UnlockPatch.Record();
}
