using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class LoanLedgerTests
{
	private const int Borrower = 0;

	[Theory]
	[InlineData(100, 0, 100)]
	[InlineData(100, 300, 400)]
	[InlineData(5, 10, 6)]
	[InlineData(4, 10, 4)]
	[InlineData(45, 15, 52)]
	public void Owed_adds_simple_interest_rounded_half_up(int amount, int rate, int expected)
	{
		Assert.Equal(expected, LoanLedger.Owed(amount, rate));
	}

	[Fact]
	public void Balances_group_what_is_left_to_repay_and_collect_per_player()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 2, Owed: 30), (Lender: 1, Owed: 20) });
		ledger.AddLoan(Borrower, new[] { (Lender: 2, Owed: 5) });
		ledger.AddLoan(3, new[] { (Lender: Borrower, Owed: 12) });

		Assert.Equal(new[] { (1, 20), (2, 35) }, ledger.Balances(Borrower, borrower: true));
		Assert.Equal(new[] { (3, 12) }, ledger.Balances(Borrower, borrower: false));
		Assert.Empty(ledger.Balances(1, borrower: true));
	}

	[Fact]
	public void Repay_without_debt_keeps_everything()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(1, new[] { (Lender: 0, Owed: 20) });

		var (kept, payments) = ledger.Repay(Borrower, 30);

		Assert.Equal(30, kept);
		Assert.Empty(payments);
		Assert.Equal(20, ledger.DebtOf(1));
	}

	[Fact]
	public void Repay_takes_all_the_gold_until_the_debt_is_paid()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 50) });

		var first = ledger.Repay(Borrower, 30);
		Assert.Equal(0, first.Kept);
		Assert.Equal(new[] { new LoanPayment(1, 30) }, first.Payments);
		Assert.Equal(20, ledger.DebtOf(Borrower));

		var second = ledger.Repay(Borrower, 80);
		Assert.Equal(60, second.Kept);
		Assert.Equal(new[] { new LoanPayment(1, 20) }, second.Payments);
		Assert.Equal(0, ledger.DebtOf(Borrower));
		Assert.Empty(ledger.Entries);
	}

	[Fact]
	public void Lenders_of_one_loan_are_repaid_in_equal_shares()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 20), (Lender: 2, Owed: 40) });

		var (kept, payments) = ledger.Repay(Borrower, 30);

		Assert.Equal(0, kept);
		Assert.Equal(new[] { new LoanPayment(1, 15), new LoanPayment(2, 15) }, payments);
	}

	[Fact]
	public void A_paid_off_lender_drops_out_and_the_rest_goes_to_the_others()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 10), (Lender: 2, Owed: 40) });

		var (_, payments) = ledger.Repay(Borrower, 30);

		Assert.Equal(new[] { new LoanPayment(1, 10), new LoanPayment(2, 20) }, payments);
		Assert.Equal(20, ledger.OwedTo(2));
		Assert.Equal(0, ledger.OwedTo(1));
	}

	[Fact]
	public void An_indivisible_remainder_goes_to_the_earliest_lenders_and_no_gold_is_lost()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(3, new[] { (Lender: 0, Owed: 10), (Lender: 1, Owed: 10), (Lender: 2, Owed: 10) });

		var (kept, payments) = ledger.Repay(3, 1);

		Assert.Equal(new[] { new LoanPayment(0, 1) }, payments);
		Assert.Equal(1, kept + payments.Sum(p => p.Amount));

		var second = ledger.Repay(3, 5);
		Assert.Equal(new[] { new LoanPayment(0, 2), new LoanPayment(1, 2), new LoanPayment(2, 1) }, second.Payments);
		Assert.Equal(5, second.Kept + second.Payments.Sum(p => p.Amount));
	}

	[Fact]
	public void The_oldest_loan_is_repaid_first()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 10) });
		ledger.AddLoan(Borrower, new[] { (Lender: 2, Owed: 10) });

		var (_, payments) = ledger.Repay(Borrower, 15);

		Assert.Equal(new[] { new LoanPayment(1, 10), new LoanPayment(2, 5) }, payments);
	}

	[Fact]
	public void Payments_to_one_lender_across_loans_are_merged()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 10) });
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 10) });

		var (kept, payments) = ledger.Repay(Borrower, 25);

		Assert.Equal(5, kept);
		Assert.Equal(new[] { new LoanPayment(1, 20) }, payments);
	}

	[Fact]
	public void Cancel_removes_only_that_borrowers_debts()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 10) });
		ledger.AddLoan(1, new[] { (Lender: 0, Owed: 7) });

		ledger.Cancel(Borrower);

		Assert.Equal(0, ledger.DebtOf(Borrower));
		Assert.Equal(7, ledger.DebtOf(1));
	}

	[Fact]
	public void Shares_owing_nothing_are_ignored()
	{
		var ledger = new LoanLedger();

		Assert.Equal(0, ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 0) }));
		Assert.Empty(ledger.Entries);
	}

	[Fact]
	public void Encode_and_decode_round_trip_and_keep_numbering_loans()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(Borrower, new[] { (Lender: 1, Owed: 12), (Lender: 2, Owed: 30) });
		ledger.AddLoan(2, new[] { (Lender: 0, Owed: 5) });

		LoanLedger copy = LoanLedger.Decode(ledger.Encode());

		Assert.Equal(ledger.Entries, copy.Entries);
		Assert.Equal(3, copy.AddLoan(1, new[] { (Lender: 0, Owed: 1) }));
	}

	[Fact]
	public void Decode_of_nothing_is_empty_and_a_broken_array_throws()
	{
		Assert.Empty(LoanLedger.Decode(null).Entries);
		Assert.Empty(LoanLedger.Decode(Array.Empty<int>()).Entries);
		Assert.Throws<ArgumentException>(() => LoanLedger.Decode(new[] { 1, 2, 3, 4, 5 }));
	}
}
