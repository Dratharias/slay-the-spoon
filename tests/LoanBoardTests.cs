using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class LoanBoardTests
{
	private const int Borrower = 0;

	private static int Rich(int lender) => 999;

	private static LoanBoard WithRequest(int missing)
	{
		var board = new LoanBoard();
		board.Request(Borrower, missing);
		return board;
	}

	[Fact]
	public void An_offer_without_a_request_is_ignored()
	{
		var board = new LoanBoard();
		board.Offer(Borrower, new LoanOffer(1, 20, 0, false));

		Assert.Empty(board.OffersFor(Borrower));
	}

	[Fact]
	public void The_borrower_cannot_offer_to_themselves()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(Borrower, 20, 0, false));

		Assert.Empty(board.OffersFor(Borrower));
	}

	[Fact]
	public void A_new_offer_from_the_same_lender_replaces_theirs_in_place()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 0, false));
		board.Offer(Borrower, new LoanOffer(2, 10, 0, false));
		board.Offer(Borrower, new LoanOffer(1, 30, 15, false));

		Assert.Equal(new[] { new LoanOffer(1, 30, 15, false), new LoanOffer(2, 10, 0, false) }, board.OffersFor(Borrower));
	}

	[Fact]
	public void An_offer_of_nothing_withdraws()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 0, false));
		board.Offer(Borrower, new LoanOffer(1, 0, 0, false));

		Assert.Empty(board.OffersFor(Borrower));
	}

	[Fact]
	public void The_rate_is_clamped_to_900_percent()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 1000, false));
		board.Offer(Borrower, new LoanOffer(2, 20, -5, false));

		Assert.Equal(new[] { 900, 0 }, board.OffersFor(Borrower).Select(o => o.RatePercent));
	}

	[Fact]
	public void A_gift_carries_no_interest()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 50, true));

		Assert.Equal(new LoanOffer(1, 20, 0, true), board.OffersFor(Borrower).Single());
	}

	[Fact]
	public void Accept_takes_only_the_picked_lenders()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 10, false));
		board.Offer(Borrower, new LoanOffer(2, 40, 0, true));

		var transfers = board.Accept(Borrower, new[] { 2 }, Rich);

		Assert.Equal(new[] { new LoanTransfer(2, 40, 0, true) }, transfers);
	}

	[Fact]
	public void Accept_stops_at_the_missing_amount()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 30, 10, false));
		board.Offer(Borrower, new LoanOffer(2, 30, 20, false));

		var transfers = board.Accept(Borrower, new[] { 1, 2 }, Rich);

		Assert.Equal(new[] { new LoanTransfer(1, 30, 10, false), new LoanTransfer(2, 10, 20, false) }, transfers);
	}

	[Fact]
	public void Accept_clamps_each_offer_to_the_lenders_current_gold_and_skips_empty_ones()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 30, 0, false));
		board.Offer(Borrower, new LoanOffer(2, 30, 0, false));
		board.Offer(Borrower, new LoanOffer(3, 30, 0, false));

		var transfers = board.Accept(Borrower, new[] { 1, 2, 3 }, lender => lender == 1 ? 12 : lender == 2 ? 0 : 999);

		Assert.Equal(new[] { new LoanTransfer(1, 12, 0, false), new LoanTransfer(3, 28, 0, false) }, transfers);
	}

	[Fact]
	public void Accept_closes_the_request()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, false));

		board.Accept(Borrower, new[] { 1 }, Rich);

		Assert.False(board.HasRequest(Borrower));
		Assert.Empty(board.OffersFor(Borrower));
		Assert.Empty(board.Accept(Borrower, new[] { 1 }, Rich));
	}

	[Fact]
	public void A_new_request_clears_the_old_offers()
	{
		LoanBoard board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, false));

		board.Request(Borrower, 25);

		Assert.Equal(25, board.Missing(Borrower));
		Assert.Empty(board.OffersFor(Borrower));
	}

	[Fact]
	public void Requests_are_listed_in_order_and_a_request_for_nothing_cancels()
	{
		var board = new LoanBoard();
		board.Request(2, 10);
		board.Request(0, 10);
		Assert.Equal(new[] { 2, 0 }, board.Borrowers);

		board.Request(2, 0);
		Assert.Equal(new[] { 0 }, board.Borrowers);
	}

	[Fact]
	public void Every_change_raises_changed_and_clear_empties_the_board()
	{
		var board = new LoanBoard();
		int changes = 0;
		board.Changed += () => changes++;

		board.Request(Borrower, 40);
		board.Offer(Borrower, new LoanOffer(1, 10, 0, false));
		board.Cancel(Borrower);
		board.Request(1, 5);
		board.Clear();

		Assert.Equal(5, changes);
		Assert.Empty(board.Borrowers);
	}

	[Fact]
	public void Acceptance_rejects_changed_terms_without_closing_or_moving_offers()
	{
		var board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, true));
		int shown = board.Revision(Borrower);
		Assert.True(board.CanAccept(Borrower, shown, 1 << 1));
		board.Offer(Borrower, new LoanOffer(1, 40, 900, false));
		Assert.False(board.CanAccept(Borrower, shown, 1 << 1));
		Assert.True(board.HasRequest(Borrower));
		Assert.Equal(40, board.OffersFor(Borrower).Single().Amount);
	}

	[Fact]
	public void Acceptance_requires_enough_selected_gold_and_a_live_request()
	{
		var board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 20, 0, false));
		board.Offer(Borrower, new LoanOffer(2, 30, 0, true));
		int shown = board.Revision(Borrower);
		Assert.False(board.CanAccept(Borrower, shown, 0));
		Assert.False(board.CanAccept(Borrower, shown, 1 << 1));
		Assert.True(board.CanAccept(Borrower, shown, (1 << 1) | (1 << 2)));
		board.Cancel(Borrower);
		Assert.False(board.CanAccept(Borrower, shown, 6));
	}

	[Fact]
	public void Recreated_request_cannot_reuse_an_old_acceptance()
	{
		var board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, false));
		int shown = board.Revision(Borrower);
		board.Clear();
		board.Request(Borrower, 40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, false));
		Assert.False(board.CanAccept(Borrower, shown, 2));
		Assert.True(board.CanAccept(Borrower, board.Revision(Borrower), 2));
	}

	[Fact]
	public void Another_borrowers_offer_does_not_invalidate_acceptance()
	{
		var board = WithRequest(40);
		board.Offer(Borrower, new LoanOffer(1, 40, 0, false));
		int shown = board.Revision(Borrower);
		board.Request(2, 20);
		board.Offer(2, new LoanOffer(1, 20, 0, false));
		Assert.True(board.CanAccept(Borrower, shown, 2));
	}
}
