namespace SlaySpoon.Core;

/// <summary>Trophies (Custom Run): an elite's attack move as a card.</summary>
public static class TrophyRule
{
	/// <summary>Damage of a Trophy from an elite with no attack move.</summary>
	public const int FallbackDamage = 10;

	/// <summary>Energy cost by total damage: 1 under 15, 2 under 30, else 3.</summary>
	public static int Cost(int damage, int hits)
	{
		int total = Math.Max(damage, 0) * Math.Max(hits, 1);
		return total < 15 ? 1 : total < 30 ? 2 : 3;
	}

	/// <summary>The upgrade adds 25 % damage, rounded up (at least 1).</summary>
	public static int UpgradeBonus(int damage) => Math.Max((Math.Max(damage, 0) + 3) / 4, 1);

	public static string Stream(int floor, int playerSlot) => $"SlaySpoon.Trophy.{floor}.{playerSlot}";
}
