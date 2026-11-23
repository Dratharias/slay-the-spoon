using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Curses.Forged;

/// <summary>
/// Odd, Even, Late and Demanding make a forged curse unplayable while their condition holds, even with the Unholy
/// Grimoire. The refusal is card logic, so the thought bubble says the defect line (<c>CantPlayMessagePatch</c>).
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay), [typeof(UnplayableReason), typeof(AbstractModel)],
	[ArgumentType.Out, ArgumentType.Out])]
public static class CurseDefectPlayPatch
{
	public static void Postfix(CardModel __instance, ref bool __result, ref UnplayableReason reason)
	{
		if (__instance.Type != CardType.Curse || !CurseDefects.BlockedNow(__instance)) return;
		reason |= UnplayableReason.BlockedByCardLogic;
		__result = false;
	}
}
