using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SlaySpoon.Cards;

/// <summary>
/// StS1 "can't play" lines: a StS1 card blocked only by its own condition (Signature Move, Clash, Grand Finale, Secret
/// Technique, Secret Weapon) says its StS1 line in the thought bubble instead of StS2's generic "I can't play this card".
/// <c>NMouseCardPlay</c> and <c>NControllerCardPlay</c> call <c>CancelPlayCard</c> on the refused card, then turn the
/// reason into a line through the internal <c>UnplayableReasonExtensions.GetPlayerDialogueLine</c>, which never sees the
/// card: the first patch remembers the card being cancelled, the second swaps its line in and forgets it.
/// </summary>
public static class CantPlayMessagePatch
{
	private static readonly Func<NCardPlay, CardModel?> CardOf =
		AccessTools.MethodDelegate<Func<NCardPlay, CardModel?>>(AccessTools.PropertyGetter(typeof(NCardPlay), "Card"));

	private static CardModel? _cancelled;

	[HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.CancelPlayCard))]
	private static class CancelPatch
	{
		private static void Prefix(NCardPlay __instance) => _cancelled = CardOf(__instance);
	}

	[HarmonyPatch]
	private static class DialoguePatch
	{
		private static System.Reflection.MethodBase TargetMethod() =>
			AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Cards.UnplayableReasonExtensions"),
				"GetPlayerDialogueLine");

		private static void Postfix(UnplayableReason reason, ref LocString? __result)
		{
			var card = _cancelled;
			_cancelled = null;
			if (reason == UnplayableReason.BlockedByCardLogic && card is Sts1Card { HasCantPlayMessage: true } sts1)
			{
				__result = sts1.CantPlayMessage;
			}
			else if (reason == UnplayableReason.BlockedByCardLogic && card != null && Curses.Forged.CurseDefects.BlockedNow(card))
			{
				__result = new LocString("cards", "SPOON_CURSE_DEFECT.cantPlay");
			}
		}
	}
}
