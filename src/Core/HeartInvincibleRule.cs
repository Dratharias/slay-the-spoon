namespace SlaySpoon.Core;

/// <summary>StS1 Heart's shared per-turn HP-loss budget. Damage caps in StS2 run before Block.</summary>
public static class HeartInvincibleRule
{
	public static int InitialCap(int ascension, int players) => (int)((ascension >= 10 ? 200 : 300)
		* (players > 1 ? players * 1.3m : 1m));

	public static decimal DamageCap(int remaining, int block, bool unblockable) =>
		Math.Max(0, remaining) + (unblockable ? 0 : Math.Max(0, block));

	/// <summary>HP actually lost from a hit: never more than the budget left, never negative.</summary>
	public static decimal ClampHpLoss(decimal amount, int remaining) => Math.Min(amount, Math.Max(0, remaining));

	public static int AfterHpLoss(int remaining, int hpLost) => Math.Max(0, remaining - Math.Max(0, hpLost));
}

/// <summary>
/// The Invincible power's budget over a fight: the cap set from <see cref="HeartInvincibleRule.InitialCap"/>, the HP
/// loss left this turn, and the refill at the Heart's turn start. The amount the game applies may be changed by power
/// hooks (or a re-apply may see a depleted amount); <see cref="SetCap"/> after the apply is what fixes the budget, so
/// the co-op cap is never scaled twice.
/// </summary>
public sealed class InvincibleBudget
{
	public int Cap { get; private set; }

	public int Remaining { get; private set; }

	/// <summary>The game applied (or re-applied) the power with <paramref name="amount"/>: never lowers the cap.</summary>
	public void Applied(int amount)
	{
		Remaining = amount;
		Cap = Math.Max(Cap, amount);
	}

	/// <summary>The budget the power refills to each turn, independent of the amount applied.</summary>
	public void SetCap(int cap)
	{
		Cap = cap;
		Remaining = cap;
	}

	public void Hit(int hpLost) => Remaining = HeartInvincibleRule.AfterHpLoss(Remaining, hpLost);

	public void TurnStart() => Remaining = Cap;
}
