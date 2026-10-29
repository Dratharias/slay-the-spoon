using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace SlaySpoon.Loans;

[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._Ready))]
public static class MerchantLoanPatch
{
	public static void Postfix(NMerchantRoom __instance)
	{
		if (MerchantLoans.IsActive(MerchantLoans.Run))
			_ = new LoanPanel(__instance);
	}
}

[HarmonyPatch(typeof(NMerchantSlot), "OnPurchaseFailed")]
public static class MerchantLoanAskPatch
{
	private static bool _asking;
	public static void Postfix(NMerchantSlot __instance, PurchaseStatus status)
	{
		if (status != PurchaseStatus.FailureGold || _asking || !MerchantLoans.IsActive(MerchantLoans.Run)
			|| MerchantLoans.Me(MerchantLoans.Run) is not { } me
			|| MerchantLoans.Board.HasRequest(MerchantLoans.IndexOf(me))) return;
		TaskHelper.RunSafely(Ask(__instance));
	}

	private static async Task Ask(NMerchantSlot slot)
	{
		var run = MerchantLoans.Run;
		var me = MerchantLoans.Me(run);
		if (me == null || NModalContainer.Instance is not { } modal) return;
		int missing = slot.Entry.Cost - me.Gold;
		if (missing <= 0 || NGenericPopup.Create() is not { } popup) return;
		_asking = true;
		try
		{
			modal.Add(popup);
			var body = new LocString("merchant_room", "SLAY_SPOON_LOANS.ask_body");
			body.Add("Missing", missing);
			bool yes = await popup.WaitForConfirmation(body, LoanPanel.Loc("title"), LoanPanel.Loc("cancel"), LoanPanel.Loc("ask"));
			if (yes && GodotObject.IsInstanceValid(slot) && slot.IsInsideTree() && MerchantLoans.Run == run
				&& !MerchantLoans.Board.HasRequest(MerchantLoans.IndexOf(me)) && slot.Entry.IsStocked)
			{
				missing = slot.Entry.Cost - me.Gold;
				if (missing > 0) MerchantLoans.Send(new NetLoanAction { Kind = LoanActionKind.Request, Amount = missing });
			}
		}
		finally { _asking = false; }
	}
}
