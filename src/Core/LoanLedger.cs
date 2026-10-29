namespace SlaySpoon.Core;

/// <summary>What one lender is still owed by one borrower on one loan. Players are indices in the run's player list.</summary>
public sealed record LoanEntry(int LoanId, int Borrower, int Lender, int Owed);

/// <summary>Gold a repayment sends to one lender.</summary>
public sealed record LoanPayment(int Lender, int Amount);

/// <summary>
/// Debts between players. Every gold a borrower gains goes to their lenders: oldest loan first, and within a loan in
/// equal shares between the lenders still owed (an indivisible remainder goes to the earliest lenders). Gifts never
/// enter the ledger.
/// </summary>
public sealed class LoanLedger
{
	private const int IntsPerEntry = 4;

	private readonly List<LoanEntry> _entries = new();
	private int _lastLoanId;

	public IReadOnlyList<LoanEntry> Entries => _entries;

	/// <summary>Amount plus simple interest, rounded half up.</summary>
	public static int Owed(int amount, int ratePercent)
	{
		return amount + (amount * ratePercent + 50) / 100;
	}

	/// <summary>Adds one loan with one entry per lender; returns its id, or 0 when no share owes anything.</summary>
	public int AddLoan(int borrower, IReadOnlyList<(int Lender, int Owed)> shares)
	{
		var owing = shares.Where(s => s.Owed > 0).ToList();
		if (owing.Count == 0)
		{
			return 0;
		}
		int loanId = ++_lastLoanId;
		foreach ((int lender, int owed) in owing)
		{
			_entries.Add(new LoanEntry(loanId, borrower, lender, owed));
		}
		return loanId;
	}

	/// <summary>Splits gold the borrower gains between their lenders; returns what the borrower keeps.</summary>
	public (int Kept, IReadOnlyList<LoanPayment> Payments) Repay(int borrower, int gained)
	{
		var paid = new List<LoanPayment>();
		int remaining = gained;
		foreach (int loanId in _entries.Where(e => e.Borrower == borrower).Select(e => e.LoanId).Distinct().ToList())
		{
			if (remaining <= 0)
			{
				break;
			}
			remaining = RepayLoan(loanId, remaining, paid);
		}
		return (remaining, paid);
	}

	private int RepayLoan(int loanId, int remaining, List<LoanPayment> paid)
	{
		while (remaining > 0)
		{
			var owed = Enumerable.Range(0, _entries.Count).Where(i => _entries[i].LoanId == loanId).ToList();
			if (owed.Count == 0)
			{
				break;
			}
			int share = remaining / owed.Count;
			foreach (int i in share == 0 ? owed.Take(remaining) : owed)
			{
				int amount = Math.Min(share == 0 ? 1 : share, _entries[i].Owed);
				_entries[i] = _entries[i] with { Owed = _entries[i].Owed - amount };
				remaining -= amount;
				Credit(paid, _entries[i].Lender, amount);
			}
			_entries.RemoveAll(e => e.Owed <= 0);
		}
		return remaining;
	}

	private static void Credit(List<LoanPayment> paid, int lender, int amount)
	{
		int index = paid.FindIndex(p => p.Lender == lender);
		if (index < 0)
		{
			paid.Add(new LoanPayment(lender, amount));
		}
		else
		{
			paid[index] = paid[index] with { Amount = paid[index].Amount + amount };
		}
	}

	/// <summary>Drops every debt of that borrower (explicit ledger reset only; never on player death).</summary>
	public void Cancel(int borrower)
	{
		_entries.RemoveAll(e => e.Borrower == borrower);
	}

	public int DebtOf(int borrower)
	{
		return _entries.Where(e => e.Borrower == borrower).Sum(e => e.Owed);
	}

	public int OwedTo(int lender)
	{
		return _entries.Where(e => e.Lender == lender).Sum(e => e.Owed);
	}

	/// <summary>Four ints per entry (loan id, borrower, lender, owed), in order; fits an <c>int[]</c> saved property.</summary>
	public int[] Encode()
	{
		return _entries.SelectMany(e => new[] { e.LoanId, e.Borrower, e.Lender, e.Owed }).ToArray();
	}

	public static LoanLedger Decode(int[]? data)
	{
		var ledger = new LoanLedger();
		if (data == null || data.Length == 0)
		{
			return ledger;
		}
		if (data.Length % IntsPerEntry != 0)
		{
			throw new ArgumentException($"loan ledger length {data.Length} is not a multiple of {IntsPerEntry}", nameof(data));
		}
		for (int i = 0; i < data.Length; i += IntsPerEntry)
		{
			ledger._entries.Add(new LoanEntry(data[i], data[i + 1], data[i + 2], data[i + 3]));
		}
		ledger._lastLoanId = ledger._entries.Max(e => e.LoanId);
		return ledger;
	}
}
