using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
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
