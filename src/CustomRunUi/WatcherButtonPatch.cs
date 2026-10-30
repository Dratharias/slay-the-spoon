using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.CustomRunUi;

/// <summary>
/// Adds a vanilla character select button for the Watcher at the end of the Custom Run screen's row. ModelDb.AllCharacters
/// stays vanilla, so Standard and Daily never see her. The button shows only while the lobby's modifiers hold a StS1
/// card set (<see cref="WatcherAvailability"/>), refreshed on every modifier change on the host and the client. A player
/// on the Watcher when she stops being available goes back to their last vanilla pick through the button's own
/// selection, which goes through the lobby's character change, so peers stay in sync.
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

	/// <summary>
	/// The local player's last vanilla pick (id entry). The screen is a single cached node, and each opening selects
	/// the first button, which resets this.
	/// </summary>
	private static string _previous = ModelIds.FromClassName(nameof(Ironclad));

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
			button.Visible = false;
			button.FocusNeighborRight = button.GetPath();
			button.FocusNeighborTop = SeedInput(__instance).GetPath();
			button.FocusNeighborBottom = button.GetPath();
			Control? left = LeftNeighbour(container, button);
			button.FocusNeighborLeft = left?.GetPath() ?? button.GetPath();
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

	[HarmonyPostfix]
	[HarmonyPatch(nameof(NCustomRunScreen.SelectCharacter))]
	public static void RememberVanillaPick(CharacterModel characterModel)
	{
		if (characterModel is not Sts1Watcher)
		{
			_previous = characterModel.Id.Entry;
		}
	}

	/// <summary>Host and singleplayer reach it through StartRunLobby.SetModifiers, the client through the sync message.</summary>
	[HarmonyPostfix]
	[HarmonyPatch(nameof(NCustomRunScreen.ModifiersChanged))]
	[HarmonyPatch(nameof(NCustomRunScreen.InitializeSingleplayer))]
	[HarmonyPatch(nameof(NCustomRunScreen.InitializeMultiplayerAsHost))]
	[HarmonyPatch(nameof(NCustomRunScreen.InitializeMultiplayerAsClient))]
	[HarmonyPatch(nameof(NCustomRunScreen.OnSubmenuOpened))]
	public static void Refresh(NCustomRunScreen __instance)
	{
		StartRunLobby? lobby = __instance.Lobby;
		if (lobby == null)
		{
			return;
		}
		Control container = ButtonContainer(__instance);
		if (container.GetNodeOrNull<NCharacterSelectButton>(ButtonName) is not { } button)
		{
			return;
		}
		bool available = WatcherAvailability.IsAvailable(lobby.Modifiers.Select(m => m.Id.Entry));
		button.Visible = available;
		Control? left = LeftNeighbour(container, button);
		if (left != null)
		{
			left.FocusNeighborRight = available ? button.GetPath() : left.GetPath();
		}

		CharacterModel? current = lobby.LocalPlayer.character;
		if (current == null)
		{
			return;
		}
		string kept = WatcherAvailability.Fallback(current.Id.Entry, _previous, available);
		if (kept == current.Id.Entry)
		{
			return;
		}
		NCharacterSelectButton? target = container.GetChildren().OfType<NCharacterSelectButton>()
			.FirstOrDefault(b => b.Character?.Id.Entry == kept)
			?? container.GetChildOrNull<NCharacterSelectButton>(0);
		if (target == null)
		{
			return;
		}
		SpoonLog.Info($"watcher: unavailable with these modifiers, back to {target.Character.Id.Entry}");
		// Select goes through NCustomRunScreen.SelectCharacter: deselects her button and StartRunLobby.SetLocalCharacter
		// sends the change to every peer.
		target.Select();
	}

	/// <summary>The vanilla button before hers in the row.</summary>
	private static Control? LeftNeighbour(Control container, Control button)
	{
		int index = button.GetIndex();
		return index > 0 ? container.GetChild<Control>(index - 1) : null;
	}
}
