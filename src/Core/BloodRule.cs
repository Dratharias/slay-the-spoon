namespace SlaySpoon.Core;

/// <summary>Blood (Custom Run): gold is paid in current HP at a fixed rate; Blood Fang heals part of attack damage.</summary>
public static class BloodRule
{
	/// <summary>Gold worth one HP.</summary>
	public const int Rate = 5;

	/// <summary>Percent of unblocked attack damage Blood Fang heals.</summary>
	public const int LeechPercent = 20;

	/// <summary>The gold a player can spend: every HP but the last.</summary>
	public static int SpendableGold(int currentHp) => Math.Max(currentHp - 1, 0) * Rate;

	/// <summary>HP a payment of <paramref name="gold"/> costs, rounded up, never more than the spendable HP.</summary>
	public static int HpCost(int gold, int currentHp) => Math.Clamp((gold + Rate - 1) / Rate, 0, Math.Max(currentHp - 1, 0));

	public static int Leech(int unblockedDamage) => Math.Max(unblockedDamage, 0) * LeechPercent / 100;
}
