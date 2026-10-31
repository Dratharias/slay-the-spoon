using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Hooks;

namespace SlaySpoon.Watcher;

/// <summary>Purge copies without the extra restrictions of StS2 IsDupe (which suppresses further duplication).</summary>
[HarmonyPatch]
internal static class OmnisciencePurge
{
	private static readonly ConditionalWeakTable<CardModel, object> Marked = new();
	internal static IDisposable Mark(CardModel card)
	{
		Marked.Add(card, new object());
		return new Scope(card);
	}
	private sealed class Scope(CardModel card) : IDisposable
	{
		public void Dispose() => Marked.Remove(card);
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyCardPlayResultPileTypeAndPosition))]
	[HarmonyPostfix]
	private static void Result(CardModel card, ref (PileType, CardPilePosition) __result)
	{
		if (Marked.TryGetValue(card, out _)) __result = (PileType.None, __result.Item2);
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.MoveToResultPileWithoutPlaying))]
	[HarmonyPrefix]
	private static bool Unplayable(CardModel __instance, ref Task __result)
	{
		if (!Marked.TryGetValue(__instance, out _)) return true;
		__result = CardPileCmd.RemoveFromCombat(__instance);
		return false;
	}
}
