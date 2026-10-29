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

	/// <summary>
	/// Whether a Masterful Stab copy gets 1 costlier from an HP change: StS1 raises the cost of each copy in hand,
	/// draw or discard pile when its owner loses HP, so a copy created later starts at its base cost.
	/// </summary>
	public static bool MasterfulStabCounts(decimal hpDelta, bool ownerLostIt, bool inHandDrawOrDiscard)
	{
		return hpDelta < 0 && ownerLostIt && inHandDrawOrDiscard;
	}
}
