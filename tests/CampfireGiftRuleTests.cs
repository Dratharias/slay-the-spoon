using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class CampfireGiftRuleTests
{
	[Theory]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	[InlineData(true, true, false)]
	public void BuyerMustBeALivingAlly(bool isSeller, bool alive, bool expected) =>
		Assert.Equal(expected, CampfireGiftRule.CanReceive(isSeller, alive));

	[Theory]
	[InlineData("Attack", true, true)]
	[InlineData("Curse", true, false)]
	[InlineData("Skill", false, false)]
	public void CursesAndUnremovableCardsStay(string type, bool removable, bool expected) =>
		Assert.Equal(expected, CampfireGiftRule.CanGift(type, removable));

	[Theory]
	[InlineData(0, 900, 0, 0, 0)]
	[InlineData(50, 900, 100, 50, 0)]
	[InlineData(100, 50, 40, 40, 90)]
	[InlineData(100, 0, 40, 40, 60)]
	[InlineData(999, 900, 0, 0, 9990)]
	[InlineData(1, 50, 0, 0, 2)]
	public void InterestOnlyAppliesToUnpaidPrice(int price, int rate, int gold, int paid, int debt) =>
		Assert.Equal((paid, debt), CampfireGiftRule.Payment(price, rate, gold));

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(1000, 0)]
	[InlineData(10, -1)]
	[InlineData(10, 901)]
	public void InvalidTermsAreRejected(int price, int rate) =>
		Assert.Throws<ArgumentOutOfRangeException>(() => CampfireGiftRule.Payment(price, rate, 1000));

	[Fact]
	public void SaleCreditSurvivesSaveAndSharesRepaymentWithMerchantLoans()
	{
		var ledger = new LoanLedger();
		ledger.AddLoan(1, [(2, 10)]);
		var payment = CampfireGiftRule.Payment(100, 50, 40);
		ledger.AddLoan(1, [(0, payment.Debt)]);
		ledger = LoanLedger.Decode(ledger.Encode());
		var (kept, payments) = ledger.Repay(1, 50);
		Assert.Equal(0, kept);
		Assert.Equal(new[] { new LoanPayment(2, 10), new LoanPayment(0, 40) }, payments);
		Assert.Equal(50, ledger.DebtOf(1));
		Assert.Equal(50, ledger.OwedTo(0));
	}
}
