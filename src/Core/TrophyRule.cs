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

	/// <summary>
	/// Text keys to try for a move's title, in order: the game drops a "_MOVE" suffix from move state ids in its
	/// monster text (TERROR_EEL's THRASH_MOVE is titled by TERROR_EEL.moves.THRASH.title), then the raw id.
	/// </summary>
	public static IEnumerable<string> MoveTitleKeys(string monster, string move)
	{
		if (move.EndsWith("_MOVE", StringComparison.Ordinal)) yield return $"{monster}.moves.{move[..^5]}.title";
		yield return $"{monster}.moves.{move}.title";
	}

	/// <summary>The portrait the art tool renders for an elite (its bestiary pose).</summary>
	public static string PortraitPath(string monster) => $"res://SlaySpoon/images/cards/trophies/{monster.ToLowerInvariant()}.png";

	public static string Stream(int floor, int playerSlot) => $"SlaySpoon.Trophy.{floor}.{playerSlot}";
}
