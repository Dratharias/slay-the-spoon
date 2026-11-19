using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Characters;

/// <summary>Offer the Watcher in Standard solo and co-op without changing global character or random-pick lists.</summary>
[HarmonyPatch(typeof(NCharacterSelectScreen))]
public static class WatcherStandardSelectPatch
{
	private const string ButtonName = "STS1_WATCHER_button";
	private static readonly AccessTools.FieldRef<NCharacterSelectScreen, StartRunLobby> Lobby =
		AccessTools.FieldRefAccess<NCharacterSelectScreen, StartRunLobby>("_lobby");
	private static readonly AccessTools.FieldRef<NCharacterSelectScreen, NCharacterSelectButton> RandomButton =
		AccessTools.FieldRefAccess<NCharacterSelectScreen, NCharacterSelectButton>("_randomCharacterButton");
	private static Control Container(NCharacterSelectScreen screen) =>
		screen.GetNode<Control>("CharSelectButtons/ButtonContainer");

	[HarmonyPostfix]
	[HarmonyPatch("InitCharacterButtons")]
	public static void AddButton(NCharacterSelectScreen __instance)
	{
		Control container = Container(__instance);
		if (container.GetNodeOrNull(ButtonName) != null)
		{
			return;
		}
		var button = PreloadManager.Cache.GetScene("res://scenes/screens/char_select/char_select_button.tscn")
			.Instantiate<NCharacterSelectButton>();
		button.Name = ButtonName;
		container.AddChild(button);
		button.Init(ModelDb.Character<Sts1Watcher>(), __instance);
		button.DebugUnlock();
		button.Visible = false;
		// Vanilla adds Random last; keep it there, right of the Watcher (the gap then follows Random).
		Control? random = RandomButton(__instance);
		if (random != null)
		{
			container.MoveChild(button, random.GetIndex());
		}
		WatcherSelectLayout.AddRightGap(container, random ?? button).Visible = false;
	}

	[HarmonyPostfix]
	[HarmonyPatch("AfterInitialized")]
	[HarmonyPatch("UpdateRandomCharacterVisibility")]
	public static void Refresh(NCharacterSelectScreen __instance)
	{
		Control container = Container(__instance);
		if (container.GetNodeOrNull<NCharacterSelectButton>(ButtonName) is not { } button)
		{
			return;
		}
		// Vanilla walks every button in the container for remote players' picks, so co-op lobbies mark hers too.
		button.Visible = Lobby(__instance) is { } lobby && WatcherRunRule.OfferedInLobby(lobby.GameMode == GameMode.Daily);
		WatcherSelectLayout.AddRightGap(container, RandomButton(__instance) ?? button).Visible = button.Visible;
		var buttons = container.GetChildren().OfType<NCharacterSelectButton>().Where(b => b.Visible).ToList();
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].FocusNeighborLeft = buttons[(i + buttons.Count - 1) % buttons.Count].GetPath();
			buttons[i].FocusNeighborRight = buttons[(i + 1) % buttons.Count].GetPath();
			buttons[i].FocusNeighborTop = buttons[i].GetPath();
			buttons[i].FocusNeighborBottom = buttons[i].GetPath();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(NCharacterSelectScreen.SelectCharacter))]
	public static void ShowWatcher(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		if (characterModel is Sts1Watcher)
		{
			ShowPortrait(__instance);
		}
	}

	/// <summary>Random landed on her: vanilla adds her CharacterSelectBg (the Silent's, borrowed); show hers.</summary>
	[HarmonyPostfix]
	[HarmonyPatch("OnLocalCharacterChangedForRandom")]
	public static void ShowRandomWatcher(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		if (characterModel is Sts1Watcher)
		{
			ShowPortrait(__instance);
		}
	}

	private static void ShowPortrait(NCharacterSelectScreen __instance)
	{
		Control background = __instance.GetNode<Control>("AnimatedBg");
		foreach (CanvasItem child in background.GetChildren().OfType<CanvasItem>())
		{
			child.Visible = false;
		}
		// The StS1 select portrait, covering the screen like the vanilla backgrounds (1920x1200 cropped to 16:9).
		// SelectCharacter frees every AnimatedBg child first, so a new one is added each time.
		var portrait = new TextureRect
		{
			Name = "Sts1WatcherPortrait",
			Texture = ResourceLoader.Load<Texture2D>(WatcherAssets.CharacterSelectBackground),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		background.AddChild(portrait);
		portrait.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
	}
}
