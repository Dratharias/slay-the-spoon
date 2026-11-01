using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Every vanilla path that offers or generates a character card reads the pool through GetUnlockedCards:
/// rewards (CardCreationOptions), the merchant, transforms, and random generation by cards and powers
/// (Infernal Blade, Creative AI, ...). Reshaping its result here makes all of them follow the card set mode.
/// The mode is per player (Spire Souvenir); the player is found through the UnlockState the caller passes.
/// </summary>
[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
public static class UnlockedCardsPatch
{
	private static readonly Func<RunManager, RunState?> RunState =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	/// <summary>Non-zero while vanilla results are wanted: inside Compose, and in the card library.</summary>
	[ThreadStatic]
	private static int _bypass;

	public static void Postfix(CardPoolModel __instance, UnlockState unlockState,
		CardMultiplayerConstraint multiplayerConstraint, ref IEnumerable<CardModel> __result)
	{
		if (_bypass > 0 || !CardSetPools.HasPair(__instance))
		{
			return;
		}
		RunState? run = RunState(RunManager.Instance);
		CardSetMode mode = CardSetPools.ModeFor(CardSetPools.PlayerOf(run, unlockState), run);
		if (mode == CardSetMode.Sts2Only)
		{
			return;
		}
		_bypass++;
		try
		{
			__result = CardSetPools.Compose(__instance, mode, pool => pool.GetUnlockedCards(unlockState, multiplayerConstraint));
		}
		finally
		{
			_bypass--;
		}
	}

	/// <summary>
	/// Dusty Tome (Darv's offer) picks an Ancient card from the character's pool. No StS1 card is Ancient and the
	/// StS1-only mode drops vanilla Ancients, so the pick came back null and crashed the event. The Tome reads the
	/// vanilla pool instead (owner call, 2026-11-01): same RNG draw, same card as an unmodded run.
	/// </summary>
	[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
	public static class DustyTomeBypass
	{
		public static void Prefix() => _bypass++;

		public static void Finalizer() => _bypass--;
	}

	/// <summary>The card library shows vanilla unlocks, even when opened during a run.</summary>
	[HarmonyPatch(typeof(NCardLibraryGrid), nameof(NCardLibraryGrid.RefreshVisibility))]
	public static class CardLibraryBypass
	{
		public static void Prefix() => _bypass++;

		public static void Finalizer() => _bypass--;
	}
}
