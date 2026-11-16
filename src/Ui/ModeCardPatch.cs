using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;

namespace SlaySpoon.Ui;

/// <summary>
/// Mode cards: extra cards beside Standard / Daily / Custom that start a Standard run carrying a mode's modifier as a
/// flag (owner-approved Standard exceptions). Folie à deux needs two players, so it is on the multiplayer host screen
/// only; Slay the Spoon is on both the solo and the host screen.
/// </summary>
public static class ModeLaunch
{
	internal sealed record Mode(string NodeName, string LocPrefix, string ArtPath, Type Modifier, bool Solo);

	internal static readonly Mode[] Modes =
	{
		new("SpoonFolieButton", "SPOON_FOLIE_MP", "res://SlaySpoon/images/cards/colorless/sts1_madness.png",
			typeof(SpoonFolieADeux), Solo: false),
		new("SpoonSpoonButton", "SPOON_SPOON_MODE", "res://SlaySpoon/images/cards/spoon/spoon_spoonful.png",
			typeof(SpoonSpoon), Solo: true),
	};

	/// <summary>The modifier the next Standard lobby begins with, set by a mode card, cleared by the vanilla cards.</summary>
	internal static Type? Requested;

	/// <summary>
	/// Copies the menu's Standard card once per mode (no signal copies: the Standard card's own handler must not come
	/// along), sets its art and text, and lays the row out again, scaled down when it would not fit the screen.
	/// </summary>
	internal static void AddCards(Control menu, bool solo, Action<NButton> openStandard)
	{
		var standard = menu.GetNode<NSubmenuButton>("StandardButton");
		var daily = menu.GetNode<NSubmenuButton>("DailyButton");
		var custom = menu.GetNode<NSubmenuButton>("CustomRunButton");
		foreach (NSubmenuButton vanilla in new[] { standard, daily, custom })
		{
			vanilla.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => Requested = null));
		}

		var row = new List<NSubmenuButton> { standard, daily, custom };
		float step = custom.Position.X - daily.Position.X;
		Vector2 middle = daily.Position;
		Node parent = custom.GetParent();
		foreach (Mode mode in Modes.Where(m => !solo || m.Solo))
		{
			var button = (NSubmenuButton)standard.Duplicate((int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts
				| Node.DuplicateFlags.UseInstantiation));
			button.Name = mode.NodeName;
			// The hover shader is a resource shared with the Standard card; each card needs its own.
			var panel = button.GetNode<Control>("BgPanel");
			if (panel.Material is Material material)
			{
				panel.Material = (Material)material.Duplicate();
			}
			if (ResourceLoader.Exists(mode.ArtPath))
			{
				var icon = button.GetNode<TextureRect>("Icon");
				icon.Texture = ResourceLoader.Load<Texture2D>(mode.ArtPath);
				icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
			}
			NSubmenuButton previous = row[^1];
			parent.AddChild(button);
			parent.MoveChild(button, previous.GetIndex() + 1);
			button.SetIconAndLocalization(mode.LocPrefix);
			Type modifier = mode.Modifier;
			button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(b =>
			{
				Requested = modifier;
				openStandard(b);
			}));
			previous.FocusNeighborRight = button.GetPath();
			button.FocusNeighborLeft = previous.GetPath();
			row.Add(button);
		}

		if (parent is Container || row.Count == 3)
		{
			return;
		}
		// Free-placed cards: centre the row on the old middle card, shrinking it to fit the screen width.
		Vector2 size = standard.Size * standard.Scale;
		float available = menu.GetViewportRect().Size.X - 2 * Math.Max(0f, size.X / 4);
		float scale = Math.Min(1f, available / (step * row.Count));
		for (int i = 0; i < row.Count; i++)
		{
			NSubmenuButton card = row[i];
			card.Scale = standard.Scale * scale;
			card.Position = middle + new Vector2((i - (row.Count - 1) / 2f) * step * scale, 0f) + size * (1f - scale) / 2f;
		}
	}
}

/// <summary>The multiplayer host screen: Folie à deux and Slay the Spoon open the vanilla Standard lobby.</summary>
[HarmonyPatch(typeof(NMultiplayerHostSubmenu), nameof(NMultiplayerHostSubmenu._Ready))]
internal static class HostModeCardsPatch
{
	private static void Postfix(NMultiplayerHostSubmenu __instance)
	{
		try
		{
			ModeLaunch.AddCards(__instance, solo: false, _ => __instance.StartHost(GameMode.Standard));
		}
		catch (Exception e)
		{
			SpoonLog.Error($"mode cards: host screen failed: {e.Message}");
		}
	}
}

/// <summary>The solo screen: Slay the Spoon opens the vanilla Standard character select.</summary>
[HarmonyPatch(typeof(NSingleplayerSubmenu), nameof(NSingleplayerSubmenu._Ready))]
internal static class SoloModeCardsPatch
{
	private static readonly Action<NSingleplayerSubmenu, NButton> OpenCharacterSelect =
		AccessTools.MethodDelegate<Action<NSingleplayerSubmenu, NButton>>(
			AccessTools.Method(typeof(NSingleplayerSubmenu), "OpenCharacterSelect"));

	private static void Postfix(NSingleplayerSubmenu __instance)
	{
		try
		{
			ModeLaunch.AddCards(__instance, solo: true, b => OpenCharacterSelect(__instance, b));
		}
		catch (Exception e)
		{
			SpoonLog.Error($"mode cards: solo screen failed: {e.Message}");
		}
	}
}

/// <summary>Coming back to the solo or host screen forgets an earlier mode pick.</summary>
[HarmonyPatch]
internal static class ModeCardsOpenedPatch
{
	private static IEnumerable<System.Reflection.MethodBase> TargetMethods() => new[]
	{
		AccessTools.Method(typeof(NMultiplayerHostSubmenu), nameof(NMultiplayerHostSubmenu.OnSubmenuOpened)),
		AccessTools.Method(typeof(NSingleplayerSubmenu), nameof(NSingleplayerSubmenu.OnSubmenuOpened)),
	};

	private static void Postfix() => ModeLaunch.Requested = null;
}

/// <summary>Host or solo: a Standard lobby opened from a mode card begins its run with that mode's modifier.</summary>
[HarmonyPatch(typeof(StartRunLobby), "BeginRunForAllPlayers")]
public static class ModeLobbyPatch
{
	private static readonly AccessTools.FieldRef<StartRunLobby, bool> BeginningRun =
		AccessTools.FieldRefAccess<StartRunLobby, bool>("_isBeginningRun");

	public static void Prefix(StartRunLobby __instance, ref List<ModifierModel> modifiers)
	{
		try
		{
			Type? requested = ModeLaunch.Requested;
			if (requested == null || __instance.GameMode != GameMode.Standard
				|| __instance.NetService.Type is not (NetGameType.Host or NetGameType.Singleplayer)
				|| BeginningRun(__instance) || modifiers.Any(m => m.GetType() == requested))
			{
				return;
			}
			ModifierModel flag = ModelDb.GetById<ModifierModel>(ModelDb.GetId(requested)).ToMutable();
			modifiers = new List<ModifierModel>(modifiers) { flag };
			ModeLaunch.Requested = null;
			SpoonLog.Info($"mode cards: {requested.Name} added to the Standard run");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"mode cards: could not add the run flag: {e.Message}");
		}
	}
}
