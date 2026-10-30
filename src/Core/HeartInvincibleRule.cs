namespace SlaySpoon.Core;

/// <summary>StS1 Heart's shared per-turn HP-loss budget. Damage caps in StS2 run before Block.</summary>
public static class HeartInvincibleRule
{
	public static int InitialCap(int ascension, int players) => (int)((ascension >= 10 ? 200 : 300)
		* (players > 1 ? players * 1.3m : 1m));

	public static decimal DamageCap(int remaining, int block, bool unblockable) =>
		Math.Max(0, remaining) + (unblockable ? 0 : Math.Max(0, block));

	public static int AfterHpLoss(int remaining, int hpLost) => Math.Max(0, remaining - Math.Max(0, hpLost));
}
