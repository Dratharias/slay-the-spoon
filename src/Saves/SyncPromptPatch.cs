using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;

namespace SlaySpoon.Saves;

/// <summary>
/// The first time the main menu shows with the mod, ask whether modded runs should use the regular
/// progress. The answer is saved in mod_configs/SlaySpoon.cfg; deleting that file asks again.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class SyncPromptPatch
{
	private static bool _askedThisSession;

	public static void Postfix()
	{
		if (_askedThisSession || !UserDataPathProvider.IsRunningModded || SpoonConfigFile.SyncChoice != null)
		{
			return;
		}
		_askedThisSession = true;
		TaskHelper.RunSafely(AskAsync());
	}

	private static async Task AskAsync()
	{
		NGenericPopup? popup = NGenericPopup.Create();
		if (popup == null)
		{
			return;
		}
		NModalContainer.Instance?.Add(popup);
		bool sync = await popup.WaitForConfirmation(
			new LocString("main_menu_ui", "SLAY_SPOON_SYNC.body"),
			new LocString("main_menu_ui", "SLAY_SPOON_SYNC.title"),
			new LocString("main_menu_ui", "SLAY_SPOON_SYNC.no"),
			new LocString("main_menu_ui", "SLAY_SPOON_SYNC.yes"));
		SpoonConfigFile.SaveSyncChoice(sync);
		SpoonLog.Info($"progress sync choice saved: {sync}");
		if (sync)
		{
			// Copies through ProgressSyncPatch and reloads, so the choice applies without a restart.
			SaveManager.Instance.InitProgressData();
		}
	}
}
