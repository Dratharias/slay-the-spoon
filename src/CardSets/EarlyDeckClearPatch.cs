using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.CardSets;

/// <summary>
/// Sealed Deck, Draft and Insanity clear every deck in their own <c>OnRunCreated</c>, in the run's modifier order, so a
/// start card a mod modifier added before them (the Spoonful, Cursed Deck's curse) was wiped. With a clearing modifier
/// in the run, the decks are cleared once before any modifier runs, and the clearing modifiers skip their own clear
/// (the rest of their <c>OnRunCreated</c> runs as vanilla).
/// </summary>
[HarmonyPatch]
public static class EarlyDeckClearPatch
{
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));
	private static readonly AccessTools.FieldRef<ModifierModel, RunState?> ModifierRunState =
		AccessTools.FieldRefAccess<ModifierModel, RunState?>("_runState");
	private static readonly MethodInfo AfterRunCreated = AccessTools.Method(typeof(ModifierModel), "AfterRunCreated");

	private static RunState? _clearedEarly;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RunManager), "InitializeNewRun")]
	public static void ClearFirst(RunManager __instance)
	{
		_clearedEarly = null;
		var state = State(__instance);
		if (state == null || !state.Modifiers.Any(m => m.ClearsPlayerDeck)) return;
		foreach (var player in state.Players) player.Deck.Clear();
		_clearedEarly = state;
		SpoonLog.Info("start decks cleared before the modifiers run (Sealed Deck, Draft or Insanity)");
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunManager), "InitializeNewRun")]
	public static void Done() => _clearedEarly = null;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ModifierModel), nameof(ModifierModel.OnRunCreated))]
	public static bool SkipSecondClear(ModifierModel __instance, RunState runState)
	{
		if (!__instance.ClearsPlayerDeck || !ReferenceEquals(_clearedEarly, runState)) return true;
		__instance.AssertMutable();
		ModifierRunState(__instance) = runState;
		AfterRunCreated.Invoke(__instance, new object[] { runState });
		return false;
	}
}
