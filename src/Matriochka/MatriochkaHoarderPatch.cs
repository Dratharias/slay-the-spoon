using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using SlaySpoon.Config;

namespace SlaySpoon.Matriochka;

/// <summary>
/// Matriochka already applies Hoarder's rule (two extra copies of every card that enters the deck, no merchant removal).
/// With vanilla Hoarder ticked too, each card came in 5 copies; the modes stay compatible and Hoarder's copies are
/// skipped instead (owner, 2026-11-23).
/// </summary>
[HarmonyPatch(typeof(Hoarder), nameof(Hoarder.AfterCardChangedPiles))]
public static class MatriochkaHoarderPatch
{
	public static bool Prefix(CardModel card, ref Task __result)
	{
		if (SpoonMatriochka.Of(card.Owner?.RunState) == null)
			return true;
		__result = Task.CompletedTask;
		return false;
	}
}
