using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SlaySpoon.Combat;

/// <summary>
/// Works around a vanilla co-op race (found by the bot tester, 1.8.20). A card that pauses for a player choice with
/// <c>CancelPlayCardActions</c> (Photon Cut) sets its queue's "cancelling play card actions" marker. If the combat ends
/// in the same frame, the end-of-combat cancel removes the card from the queue first, then the pause sets the marker on
/// an action that is no longer queued, so nothing ever clears it. On that peer every later card play from the player is
/// dropped as "cancelling all play card actions due to player choice", including plays the host already ran: the
/// next combat diverges. Vanilla clears the marker whenever its action leaves the queue, so a marker whose action is
/// not in the queue is stale; it is cleared here before a card play is enqueued.
/// </summary>
[HarmonyPatch(typeof(ActionQueueSet), nameof(ActionQueueSet.EnqueueWithoutSynchronizing))]
public static class StaleCardCancelPatch
{
	private static readonly MethodInfo GetQueue = AccessTools.Method(typeof(ActionQueueSet), "GetQueue");
	private static readonly Type QueueType = AccessTools.Inner(typeof(ActionQueueSet), "ActionQueue");
	private static readonly FieldInfo Marker = AccessTools.Field(QueueType, "actionCancellingPlayCardActions");
	private static readonly FieldInfo Actions = AccessTools.Field(QueueType, "actions");

	public static void Prefix(ActionQueueSet __instance, GameAction gameAction)
	{
		if (gameAction is not PlayCardAction)
			return;
		try
		{
			object? queue = GetQueue.Invoke(__instance, [gameAction.OwnerId]);
			if (queue == null || Marker.GetValue(queue) is not GameAction marker)
				return;
			if (Actions.GetValue(queue) is IList actions && actions.Contains(marker))
				return;
			Marker.SetValue(queue, null);
			SpoonLog.Info($"stale card-play cancel marker cleared for player {gameAction.OwnerId} (left by {marker})");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"stale card-play cancel check failed: {e.Message}");
		}
	}
}
