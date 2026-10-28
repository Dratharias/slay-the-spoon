using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SlaySpoon.MainMenu;

/// <summary>
/// Slay the Spoon's settings only exist on the Custom Run screen, so Standard and Daily runs are
/// locked in both the solo and the multiplayer host menus. Each submenu re-enables its buttons in
/// RefreshButtons whenever it opens; lock them again right after.
/// </summary>
[HarmonyPatch]
public static class RunModeLockPatch
{
	private static readonly AccessTools.FieldRef<NSingleplayerSubmenu, NSubmenuButton> SoloStandard =
		AccessTools.FieldRefAccess<NSingleplayerSubmenu, NSubmenuButton>("_standardButton");
	private static readonly AccessTools.FieldRef<NSingleplayerSubmenu, NSubmenuButton> SoloDaily =
		AccessTools.FieldRefAccess<NSingleplayerSubmenu, NSubmenuButton>("_dailyButton");
	private static readonly AccessTools.FieldRef<NSingleplayerSubmenu, NSubmenuButton> SoloCustom =
		AccessTools.FieldRefAccess<NSingleplayerSubmenu, NSubmenuButton>("_customButton");
	private static readonly AccessTools.FieldRef<NMultiplayerHostSubmenu, NSubmenuButton> HostStandard =
		AccessTools.FieldRefAccess<NMultiplayerHostSubmenu, NSubmenuButton>("_standardButton");
	private static readonly AccessTools.FieldRef<NMultiplayerHostSubmenu, NSubmenuButton> HostDaily =
		AccessTools.FieldRefAccess<NMultiplayerHostSubmenu, NSubmenuButton>("_dailyButton");
	private static readonly AccessTools.FieldRef<NMultiplayerHostSubmenu, NSubmenuButton> HostCustom =
		AccessTools.FieldRefAccess<NMultiplayerHostSubmenu, NSubmenuButton>("_customButton");

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NSingleplayerSubmenu), "RefreshButtons")]
	public static void SoloPostfix(NSingleplayerSubmenu __instance)
	{
		Lock(SoloStandard(__instance), SoloDaily(__instance));
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NMultiplayerHostSubmenu), "RefreshButtons")]
	public static void HostPostfix(NMultiplayerHostSubmenu __instance)
	{
		Lock(HostStandard(__instance), HostDaily(__instance));
	}

	// The game focuses Standard when a submenu opens; a locked button can't take the controller focus.
	[HarmonyPostfix]
	[HarmonyPatch(typeof(NSingleplayerSubmenu), "InitialFocusedControl", MethodType.Getter)]
	public static void SoloFocusPostfix(NSingleplayerSubmenu __instance, ref Control __result)
	{
		__result = SoloCustom(__instance);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NMultiplayerHostSubmenu), "InitialFocusedControl", MethodType.Getter)]
	public static void HostFocusPostfix(NMultiplayerHostSubmenu __instance, ref Control __result)
	{
		__result = HostCustom(__instance);
	}

	private static void Lock(params NSubmenuButton[] buttons)
	{
		foreach (NSubmenuButton button in buttons)
		{
			button.SetEnabled(false);
			button.RefreshLabels();
		}
	}
}
