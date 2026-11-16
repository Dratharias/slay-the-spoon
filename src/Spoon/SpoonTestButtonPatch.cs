using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;

namespace SlaySpoon.Spoon;

/// <summary>
/// "Test: The Spoon" under Multiplayer on the main menu (a copy of the Singleplayer button, without its handler).
/// Only a mod loaded from the game's <c>mods/</c> folder shows it (<see cref="SpoonTestStart.ButtonAllowed"/>), and only
/// while no run is saved, like Singleplayer: a new run would replace the saved one.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class SpoonTestButtonPatch
{
	internal const string NodeName = "SpoonTestButton";
	internal const string LocKey = "SPOON_TEST_SPOON";

	private static void Postfix(NMainMenu __instance)
	{
		if (!SpoonTestStart.ButtonAllowed) return;
		try
		{
			var container = __instance.GetNode<Control>("MainMenuTextButtons");
			var single = container.GetNode<NMainMenuTextButton>("SingleplayerButton");
			var multi = container.GetNode<NMainMenuTextButton>("MultiplayerButton");
			var button = (NMainMenuTextButton)single.Duplicate((int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts
				| Node.DuplicateFlags.UseInstantiation));
			button.Name = NodeName;
			container.AddChild(button);
			container.MoveChild(button, multi.GetIndex() + 1);
			button.SetLocalization(LocKey);
			button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ =>
			{
				button.Disable();
				TaskHelper.RunSafely(SpoonTestStart.Start());
			}));
			ConnectReticle(__instance, button);
			Refresh(__instance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon test: main menu button failed: {e.Message}");
		}
	}

	/// <summary>The gold carets beside a focused main menu button, as vanilla connects them for its own buttons.</summary>
	private static void ConnectReticle(NMainMenu menu, NMainMenuTextButton button)
	{
		var focused = AccessTools.Method(typeof(NMainMenu), "MainMenuButtonFocused");
		var unfocused = AccessTools.Method(typeof(NMainMenu), "MainMenuButtonUnfocused");
		if (focused == null || unfocused == null) return;
		button.Connect(NClickableControl.SignalName.Focused, Callable.From<NMainMenuTextButton>(b =>
			Callable.From(() => focused.Invoke(menu, [b])).CallDeferred()));
		button.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NMainMenuTextButton>(b => unfocused.Invoke(menu, [b])));
	}

	internal static void Refresh(NMainMenu menu)
	{
		if (menu.GetNodeOrNull<NMainMenuTextButton>("MainMenuTextButtons/" + NodeName) is not { } button) return;
		bool free = !SaveManager.Instance.HasRunSave;
		button.Visible = free;
		button.SetEnabled(free);
	}
}

/// <summary>Abandoning a run from the main menu brings the button back.</summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu.RefreshButtons))]
internal static class SpoonTestButtonRefreshPatch
{
	private static void Postfix(NMainMenu __instance)
	{
		try
		{
			SpoonTestButtonPatch.Refresh(__instance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon test: main menu refresh failed: {e.Message}");
		}
	}
}
