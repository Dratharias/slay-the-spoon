using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Loans;

/// <summary>
/// Merchant loans glue: the shared board of requests and offers, and how each synced <see cref="LoanAction"/> changes
/// it, the players' gold and the debts. Every client applies the same actions in the host's order.
///
/// An offer's gold leaves the lender when the offer is made (held on the board) and goes back when it is withdrawn,
/// not taken, or when the room is left. Gold only moves by plain additions, so a merchant purchase synced in a
/// different order on another client cannot change any amount moved here.
/// </summary>
public static class MerchantLoans
{
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	/// <summary>The run the board's held gold belongs to; a board left over from another run is dropped, not refunded.</summary>
	private static IRunState? _boardRun;

	public static LoanBoard Board { get; } = new();

	/// <summary>Raised after the debts change (loan taken, repayment).</summary>
	public static event Action? Changed;
	public static event Action<int, string, int, int>? Receipt;
	private static void Notify(int player, string key, int amount = 0, int debt = 0) =>
		Receipt?.Invoke(player, key, amount, debt);

	public static IRunState? Run => State(RunManager.Instance);

	public static SpoonMerchantLoans? ModifierOf(IRunState? run)
	{
		return run?.Modifiers.OfType<SpoonMerchantLoans>().FirstOrDefault();
	}

	/// <summary>Both options share one debt queue when enabled together; campfire credit also works on its own.</summary>
	public static LoanLedger? LedgerOf(IRunState? run) => ModifierOf(run)?.Ledger
		?? run?.Modifiers.OfType<SpoonCampfireGift>().FirstOrDefault()?.Ledger;

	/// <summary>Loans need the modifier and someone to borrow from.</summary>
	public static bool IsActive(IRunState? run)
	{
		return ModifierOf(run) != null && run!.Players.Count >= 2;
	}

	public static int IndexOf(Player player)
	{
		IReadOnlyList<Player> players = player.RunState.Players;
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i] == player)
			{
				return i;
			}
		}
		return -1;
	}

	public static Player? PlayerAt(IRunState run, int index)
	{
		return index >= 0 && index < run.Players.Count ? run.Players[index] : null;
	}

	public static Player? Me(IRunState? run)
	{
		return run == null ? null : LocalContext.GetMe(run);
	}

	public static string NameOf(Player player)
	{
		return PlatformUtil.GetPlayerNameRaw(RunManager.Instance.NetService.Platform, player.NetId);
	}

	/// <summary>Queues the action; false when there is no local player to send it as.</summary>
	public static bool Send(NetLoanAction action)
	{
		if (Me(Run) is not { } me) return false;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new LoanAction(me, action));
		return true;
	}

	public static void Apply(Player sender, NetLoanAction action)
	{
		IRunState run = sender.RunState;
		if (!IsActive(run))
		{
			return;
		}
		AdoptBoard(run);
		int me = IndexOf(sender);
		switch (action.Kind)
		{
			case LoanActionKind.Request:
				RefundAll(run, me);
				Board.Request(me, action.Amount);
				Notify(me, "request_sent");
				break;
			case LoanActionKind.Cancel:
				RefundAll(run, me);
				Board.Cancel(me);
				Notify(me, "request_cancelled");
				break;
			case LoanActionKind.Offer:
				int before = OfferOf(action.Borrower, me);
				if (!Board.HasRequest(action.Borrower) || action.Borrower == me
					|| action.Amount < 0 || action.Amount > (long)sender.Gold + before)
				{
					Notify(me, "offer_rejected");
					return;
				}
				Board.Offer(action.Borrower, new LoanOffer(me, action.Amount, action.RatePercent, action.Gift));
				sender.Gold -= OfferOf(action.Borrower, me) - before;
				Notify(me, action.Amount == 0 ? "offer_withdrawn" : "offer_sent", action.Amount);
				break;
			case LoanActionKind.Accept:
				if (!Board.CanAccept(me, action.Revision, action.Lenders))
				{
					Notify(me, "terms_changed");
					return;
				}
				Accept(run, sender, me, action.Lenders);
				break;
		}
		SpoonLog.Info($"loans: {NameOf(sender)} {action}; board {Describe(run)}");
	}

	/// <summary>Leaving a room gives every held offer back (before the next room's autosave) and empties the board.</summary>
	public static void CloseBoard(IRunState run)
	{
		if (_boardRun == run)
		{
			foreach (int borrower in Board.Borrowers.ToList())
			{
				RefundAll(run, borrower);
			}
		}
		_boardRun = run;
		if (Board.Borrowers.Count > 0)
		{
			Board.Clear();
		}
	}

	public static void LedgerChanged(LoanLedger ledger, string why)
	{
		SpoonLog.Info($"loans: {why}; ledger [{string.Join(",", ledger.Encode())}]");
		Changed?.Invoke();
	}

	private static void AdoptBoard(IRunState run)
	{
		if (_boardRun != run)
		{
			_boardRun = run;
			Board.Clear();
		}
	}

	private static void Accept(IRunState run, Player borrower, int borrowerIndex, int lenderMask)
	{
		var picked = Enumerable.Range(0, run.Players.Count).Where(i => (lenderMask & (1 << i)) != 0).ToList();
		var offers = Board.OffersFor(borrowerIndex).ToList();
		// Held gold is already off the lenders, so nothing limits an offer but the amount still missing.
		IReadOnlyList<LoanTransfer> transfers = Board.Accept(borrowerIndex, picked, _ => int.MaxValue);
		foreach (LoanOffer offer in offers)
		{
			int taken = transfers.Where(t => t.Lender == offer.Lender).Sum(t => t.Amount);
			if (PlayerAt(run, offer.Lender) is { } lender)
			{
				lender.Gold += offer.Amount - taken;
			}
		}
		borrower.Gold += transfers.Sum(t => t.Amount);
		var shares = transfers.Where(t => !t.Gift).Select(t => (t.Lender, LoanLedger.Owed(t.Amount, t.RatePercent))).ToList();
		if (ModifierOf(run) is { } modifier && modifier.Ledger.AddLoan(borrowerIndex, shares) != 0)
		{
			LedgerChanged(modifier.Ledger, $"{NameOf(borrower)} borrowed "
				+ string.Join(", ", transfers.Where(t => !t.Gift).Select(t => $"{t.Amount} at {t.RatePercent}% from player {t.Lender}")));
		}
		Notify(borrowerIndex, transfers.All(t => t.Gift) ? "gift_received" : "loan_received",
			transfers.Sum(t => t.Amount), shares.Sum(s => s.Item2));
		foreach (var transfer in transfers)
			Notify(transfer.Lender, transfer.Gift ? "gift_delivered" : "loan_delivered", transfer.Amount,
				transfer.Gift ? 0 : LoanLedger.Owed(transfer.Amount, transfer.RatePercent));
	}

	private static int OfferOf(int borrower, int lender)
	{
		return Board.OffersFor(borrower).FirstOrDefault(o => o.Lender == lender)?.Amount ?? 0;
	}

	private static void RefundAll(IRunState run, int borrower)
	{
		foreach (LoanOffer offer in Board.OffersFor(borrower))
		{
			if (PlayerAt(run, offer.Lender) is { } lender)
			{
				lender.Gold += offer.Amount;
			}
		}
	}

	private static string Describe(IRunState run)
	{
		return string.Join("; ", Board.Borrowers.Select(b => $"{b} needs {Board.Missing(b)}: "
			+ string.Join(", ", Board.OffersFor(b).Select(o => $"{o.Lender} {(o.Gift ? "gives" : "lends")} {o.Amount}"
				+ (o.Gift ? "" : $" at {o.RatePercent}%")))))
			+ $" | gold {string.Join("/", run.Players.Select(p => p.Gold))}";
	}
}
