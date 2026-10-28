using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SlaySpoon.CustomRunUi;

internal static class CustomRunScreenFields
{
	public static readonly AccessTools.FieldRef<NCustomRunScreen, NCustomRunModifiersList> ModifiersList =
		AccessTools.FieldRefAccess<NCustomRunScreen, NCustomRunModifiersList>("_modifiersList");
}

/// <summary>
/// A joining client gets the host's modifiers in the join response, but the game never shows them
/// until the host changes something. Show them right away.
/// </summary>
[HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen.InitializeMultiplayerAsClient))]
public static class ClientJoinSyncPatch
{
	public static void Postfix(NCustomRunScreen __instance)
	{
		CustomRunScreenFields.ModifiersList(__instance).SyncModifierList(__instance.Lobby.Modifiers);
	}
}

/// <summary>
/// The Custom Run screen is reused, so its rows keep their ticks and percents while each opening
/// starts a fresh lobby with no modifiers. Push what the rows show into the new lobby.
/// </summary>
[HarmonyPatch(typeof(NCustomRunScreen))]
public static class HostInitPushPatch
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(NCustomRunScreen.InitializeSingleplayer))]
	[HarmonyPatch(nameof(NCustomRunScreen.InitializeMultiplayerAsHost))]
	public static void Postfix(NCustomRunScreen __instance)
	{
		__instance.Lobby.SetModifiers(CustomRunScreenFields.ModifiersList(__instance).GetModifiersTickedOn());
	}
}
