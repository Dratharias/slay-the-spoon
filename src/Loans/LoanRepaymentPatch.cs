using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Loans;

/// <summary>
/// Gold a borrower gains goes to their lenders first. This runs after every hook listener has modified the amount
/// (relics, powers), so only the gold the borrower would really get is shared out. GainGold runs on every client for
/// every player, so the debts stay the same everywhere. Lenders get plain gold, not a "gold gained" event, so it is
/// never taken for their own debts.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyGoldGained))]
public static class LoanRepaymentPatch
{
	public static void Postfix(IRunState runState, Player player, ref decimal __result)
	{
		if (__result < 1m || MerchantLoans.LedgerOf(runState) is not { } ledger)
		{
			return;
		}
		int borrower = MerchantLoans.IndexOf(player);
		if (borrower < 0 || ledger.DebtOf(borrower) == 0)
		{
			return;
		}
		int gained = (int)__result;
		(int kept, IReadOnlyList<LoanPayment> payments) = ledger.Repay(borrower, gained);
		foreach (LoanPayment payment in payments)
		{
			if (MerchantLoans.PlayerAt(runState, payment.Lender) is { } lender)
			{
				lender.Gold += payment.Amount;
			}
			else
			{
				kept += payment.Amount;
			}
		}
		__result = kept + (__result - gained);
		MerchantLoans.LedgerChanged(ledger, $"{MerchantLoans.NameOf(player)} gained {gained}, kept {kept}, repaid "
			+ string.Join(", ", payments.Select(p => $"{p.Amount} to player {p.Lender}")));
	}
}

/// <summary>Moving on gives held offers back before the next room's autosave, on every client in the action order.</summary>
[HarmonyPatch(typeof(RunManager), "ExitCurrentRooms")]
public static class LoanBoardRoomExitPatch
{
	public static void Prefix(RunManager __instance)
	{
		IRunState? run = MerchantLoans.Run;
		if (run != null && MerchantLoans.IsActive(run))
		{
			MerchantLoans.CloseBoard(run);
		}
	}
}
