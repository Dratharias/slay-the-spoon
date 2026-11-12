namespace SlaySpoon.Core;

/// <summary>Campfire sales keep the original modifier ID for existing saves.</summary>
public static class CampfireGiftRule
{
	public const int MaxPrice = 999;
	public const int MaxRate = LoanBoard.MaxRate;

	public static bool CanReceive(bool isSeller, bool alive) => !isSeller && alive;

	public static bool CanGift(string cardType, bool removable) => removable && cardType != "Curse";

	public static bool ValidTerms(int price, int rate) => price is >= 0 and <= MaxPrice && rate is >= 0 and <= MaxRate;

	/// <summary>Interest applies only to the unpaid price, using the merchant's simple-interest rounding.</summary>
	public static (int PaidNow, int Debt) Payment(int price, int rate, int gold)
	{
		if (!ValidTerms(price, rate)) throw new ArgumentOutOfRangeException(nameof(price));
		int paid = Math.Min(price, Math.Max(0, gold));
		return (paid, LoanLedger.Owed(price - paid, rate));
	}
}
