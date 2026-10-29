namespace SlaySpoon.Core;

/// <summary>StS1 card formulas that are worth a test.</summary>
public static class Sts1Math
{
	/// <summary>
	/// Searing Blow's damage after <paramref name="level"/> upgrades: 12, then +4, +5, +6 … per upgrade
	/// (StS1: n(n+7)/2 + 12).
	/// </summary>
	public static int SearingBlowDamage(int level)
	{
		return level * (level + 7) / 2 + 12;
	}
}
