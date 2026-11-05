using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Tint;

/// <summary>Each run starts with no shared colors; players share theirs again.</summary>
[HarmonyPatch(typeof(RunManager), "InitializeShared")]
internal static class TintRunStartPatch
{
	private static void Postfix() => CharacterTints.ResetRun();
}

/// <summary>Colors player creatures in combat.</summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
internal static class TintCreaturePatch
{
	private static void Postfix(NCreature __instance)
	{
		if (__instance.Entity?.Player is not { } player) return;
		if (CharacterTints.Run is { } run && LocalContext.GetMe(run) == player) CharacterTints.EnsureSent();
		CharacterTints.Track(__instance);
	}
}

/// <summary>Colors player characters at the rest site.</summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class TintRestSitePatch
{
	private static void Postfix(NRestSiteCharacter __instance) => CharacterTints.TrackBodies(__instance, __instance.Player);
}

/// <summary>Colors player characters at the merchant (after the Watcher's body is added, hence the low priority).</summary>
[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
[HarmonyPriority(Priority.Low)]
internal static class TintMerchantPatch
{
	private static readonly AccessTools.FieldRef<NMerchantRoom, List<Player>> Players =
		AccessTools.FieldRefAccess<NMerchantRoom, List<Player>>("_players");

	private static void Postfix(NMerchantRoom __instance)
	{
		List<Player> players = Players(__instance);
		IReadOnlyList<NMerchantCharacter> visuals = __instance.PlayerVisuals;
		for (int i = 0; i < players.Count && i < visuals.Count; i++) CharacterTints.TrackBodies(visuals[i], players[i]);
	}
}

/// <summary>A map ping takes the pinging player's map color, like their lines.</summary>
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.PingMapCoord))]
internal static class TintMapPingPatch
{
	private static readonly AccessTools.FieldRef<NMapScreen, Dictionary<MapCoord, NMapPoint>> Points =
		AccessTools.FieldRefAccess<NMapScreen, Dictionary<MapCoord, NMapPoint>>("_mapPointDictionary");

	private const string Done = "spoon_tinted";

	private static void Postfix(NMapScreen __instance, MapCoord coord, Player player)
	{
		if (!Points(__instance).TryGetValue(coord, out NMapPoint? point)) return;
		Color raw = player.Character.MapDrawingColor;
		// The ping was just added (possibly deferred to the end of the frame); recolor the untouched ones in the raw color.
		Callable.From(() =>
		{
			if (!GodotObject.IsInstanceValid(point)) return;
			foreach (NMapPingVfx ping in point.GetChildren().OfType<NMapPingVfx>())
			{
				if (ping.HasMeta(Done) || ping.Modulate != raw) continue;
				ping.SetMeta(Done, true);
				ping.Modulate = CharacterTints.MapColor(player, raw);
			}
		}).CallDeferred();
	}
}

/// <summary>A player's map lines (and the line being drawn) take their color when they chose so.</summary>
[HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
internal static class TintMapLinePatch
{
	private static void Postfix(Player player, bool isErasing, Line2D __result)
	{
		if (isErasing) return;
		if (CharacterTints.Run is { } run && LocalContext.GetMe(run) == player) CharacterTints.EnsureSent();
		__result.DefaultColor = CharacterTints.MapColor(player, __result.DefaultColor);
	}
}

/// <summary>
/// A shared color is applied when the synchronizer would enqueue it, on every peer alike, so it takes no action id,
/// no checksum and no place in the queue (see <see cref="TintAction"/>). The host still broadcasts it first.
/// </summary>
[HarmonyPatch(typeof(ActionQueueSet), nameof(ActionQueueSet.EnqueueWithoutSynchronizing))]
internal static class TintApplyNowPatch
{
	private static bool Prefix(GameAction gameAction)
	{
		if (gameAction is not TintAction tint) return true;
		tint.ApplyNow();
		return false;
	}
}
