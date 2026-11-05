using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.CustomRunUi;

/// <summary>
/// Adds an always-visible, unlocked Watcher button to Custom Run on hosts and clients.
/// ModelDb.AllCharacters stays vanilla, so this does not affect Daily or random selection.
/// </summary>
[HarmonyPatch(typeof(NCustomRunScreen))]
public static class WatcherButtonPatch
{
	/// <summary>The vanilla button scene NCustomRunScreen instantiates for each character (in its AssetPaths).</summary>
	private const string ButtonScene = "res://scenes/screens/char_select/char_select_button.tscn";

	private static readonly string ButtonName = WatcherAssets.CharacterId + "_button";

	private static readonly AccessTools.FieldRef<NCustomRunScreen, Control> ButtonContainer =
		AccessTools.FieldRefAccess<NCustomRunScreen, Control>("_charButtonContainer");

	private static readonly AccessTools.FieldRef<NCustomRunScreen, LineEdit> SeedInput =
		AccessTools.FieldRefAccess<NCustomRunScreen, LineEdit>("_seedInput");

	/// <summary>Runs once per screen node (from _Ready); the name check keeps a reused screen at one button.</summary>
	[HarmonyPostfix]
	[HarmonyPatch("InitCharacterButtons")]
	public static void AddButton(NCustomRunScreen __instance)
	{
		NCharacterSelectButton? button = null;
		try
		{
			Control container = ButtonContainer(__instance);
			if (container.GetNodeOrNull(ButtonName) != null)
			{
				return;
			}
			button = PreloadManager.Cache.GetScene(ButtonScene)
				.Instantiate<NCharacterSelectButton>(PackedScene.GenEditState.Disabled);
			button.Name = ButtonName;
			container.AddChildSafely(button);
			button.Init(ModelDb.Character<Sts1Watcher>(), __instance);
			// Not in the vanilla unlock state, so Init locks her; she has no unlock of her own.
			button.DebugUnlock();
			button.Visible = true;
			WatcherSelectLayout.AddRightGap(container, button);
			button.FocusNeighborRight = button.GetPath();
			button.FocusNeighborTop = SeedInput(__instance).GetPath();
			button.FocusNeighborBottom = button.GetPath();
			Control? left = LeftNeighbour(container, button);
			button.FocusNeighborLeft = left?.GetPath() ?? button.GetPath();
			if (left != null)
			{
				left.FocusNeighborRight = button.GetPath();
			}
		}
		catch (Exception error)
		{
			SpoonLog.Error($"watcher: button initialization failed; Custom Run remains available without her: {error}");
			// Detach immediately: refresh must never find a half-initialized button.
			if (GodotObject.IsInstanceValid(button))
			{
				button!.GetParent()?.RemoveChild(button);
				button.QueueFree();
			}
		}
	}

	/// <summary>The vanilla button before hers in the row.</summary>
	private static Control? LeftNeighbour(Control container, Control button)
	{
		int index = button.GetIndex();
		return index > 0 ? container.GetChild<Control>(index - 1) : null;
	}
}
