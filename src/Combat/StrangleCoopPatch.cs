using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Combat;

/// <summary>
/// Lets an ally's card trigger Strangle: vanilla records only cards its applier plays, then its own
/// <c>AfterCardPlayed</c> deals the HP loss for every recorded card. This records the allies' cards too.
/// </summary>
[HarmonyPatch(typeof(StranglePower), nameof(StranglePower.BeforeCardPlayed))]
public static class StrangleCoopPatch
{
	private static readonly AccessTools.FieldRef<PowerModel, object?> InternalData =
		AccessTools.FieldRefAccess<PowerModel, object?>("_internalData");

	private static readonly FieldInfo Amounts =
		AccessTools.Field(AccessTools.Inner(typeof(StranglePower), "Data"), "amountsForPlayedCards");

	[HarmonyPrefix]
	public static void Prefix(StranglePower __instance, CardPlay cardPlay)
	{
		var applier = __instance.Applier?.Player;
		var owner = cardPlay.Card.Owner;
		if (applier == null || owner == null || owner == applier)
		{
			return;
		}
		var run = __instance.CombatState.RunState;
		if (!StrangleRule.CountsAllies(run.GameMode == GameMode.Custom, run.Players.Count))
		{
			return;
		}
		if (InternalData(__instance) is { } data && Amounts.GetValue(data) is Dictionary<CardModel, int> amounts)
		{
			amounts.TryAdd(cardPlay.Card, __instance.Amount);
		}
	}
}
