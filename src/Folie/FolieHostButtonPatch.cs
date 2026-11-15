using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;

namespace SlaySpoon.Folie;

/// <summary>The host picked the "Folie à deux" card: the Standard lobby it opens starts a run carrying the flag.</summary>
public static class FolieLaunch
{
	internal static bool Requested;

	internal const string LocPrefix = "SPOON_FOLIE_MP";

	internal const string ArtPath = "res://SlaySpoon/images/cards/colorless/sts1_madness.png";
}

/// <summary>
/// A fourth card on the multiplayer host screen, a copy of the Standard one with the Madness art. Pressing it opens the
/// vanilla Standard lobby with <see cref="FolieLaunch.Requested"/> set; the three vanilla cards clear it.
/// </summary>
[HarmonyPatch(typeof(NMultiplayerHostSubmenu), nameof(NMultiplayerHostSubmenu._Ready))]
internal static class FolieHostButtonPatch
{
	private static void Postfix(NMultiplayerHostSubmenu __instance)
	{
		try
		{
			var standard = __instance.GetNode<NSubmenuButton>("StandardButton");
			var daily = __instance.GetNode<NSubmenuButton>("DailyButton");
			var custom = __instance.GetNode<NSubmenuButton>("CustomRunButton");
			foreach (NSubmenuButton vanilla in new[] { standard, daily, custom })
			{
				vanilla.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => FolieLaunch.Requested = false));
			}

			// No signal copies: the Standard card's own handler must not come along.
			var button = (NSubmenuButton)standard.Duplicate((int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts
				| Node.DuplicateFlags.UseInstantiation));
			button.Name = "SpoonFolieButton";
			// The hover shader is a resource shared with the Standard card; each card needs its own.
			var panel = button.GetNode<Control>("BgPanel");
			if (panel.Material is Material material)
			{
				panel.Material = (Material)material.Duplicate();
			}
			if (ResourceLoader.Exists(FolieLaunch.ArtPath))
			{
				var icon = button.GetNode<TextureRect>("Icon");
				icon.Texture = ResourceLoader.Load<Texture2D>(FolieLaunch.ArtPath);
				icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
			}

			Node parent = custom.GetParent();
			parent.AddChild(button);
			parent.MoveChild(button, custom.GetIndex() + 1);
			if (parent is not Container)
			{
				// Free-placed cards: shift the row left by half a step and put the new card one step past Custom.
				float step = custom.Position.X - daily.Position.X;
				foreach (NSubmenuButton card in new[] { standard, daily, custom })
				{
					card.Position -= new Vector2(step / 2, 0);
				}
				button.Position = custom.Position + new Vector2(step, 0);
			}
			button.SetIconAndLocalization(FolieLaunch.LocPrefix);
			button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ =>
			{
				FolieLaunch.Requested = true;
				__instance.StartHost(GameMode.Standard);
			}));
			custom.FocusNeighborRight = button.GetPath();
			button.FocusNeighborLeft = custom.GetPath();
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: host card failed: {e.Message}");
		}
	}
}

/// <summary>Coming back to the host screen forgets an earlier Folie à deux pick.</summary>
[HarmonyPatch(typeof(NMultiplayerHostSubmenu), nameof(NMultiplayerHostSubmenu.OnSubmenuOpened))]
internal static class FolieHostOpenedPatch
{
	private static void Postfix() => FolieLaunch.Requested = false;
}

/// <summary>Host: a Standard lobby opened from the Folie à deux card begins its run with the flag.</summary>
[HarmonyPatch(typeof(StartRunLobby), "BeginRunForAllPlayers")]
public static class FolieLobbyPatch
{
	private static readonly AccessTools.FieldRef<StartRunLobby, bool> BeginningRun =
		AccessTools.FieldRefAccess<StartRunLobby, bool>("_isBeginningRun");

	public static void Prefix(StartRunLobby __instance, ref List<ModifierModel> modifiers)
	{
		try
		{
			if (!FolieLaunch.Requested || __instance.GameMode != GameMode.Standard || __instance.NetService.Type != NetGameType.Host
				|| BeginningRun(__instance) || modifiers.Any(m => m is SpoonFolieADeux))
			{
				return;
			}
			modifiers = new List<ModifierModel>(modifiers) { ModelDb.Modifier<SpoonFolieADeux>().ToMutable() };
			FolieLaunch.Requested = false;
			SpoonLog.Info("folie: flag added to the Standard run");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: could not add the run flag: {e.Message}");
		}
	}
}
