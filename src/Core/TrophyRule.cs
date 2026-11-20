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
	/// Text keys to try for a move's title, most trimmed first: the game's monster text drops a "_MOVE" suffix and a
	/// variant number from move state ids (TERROR_EEL's THRASH_MOVE is THRASH, SKULKING_COLONY's ZOOM_MOVE_2 is ZOOM).
	/// </summary>
	public static IEnumerable<string> MoveTitleKeys(string monster, string move)
	{
		string numberless = System.Text.RegularExpressions.Regex.Replace(move, "_?[0-9]+$", "");
		string bare = numberless.EndsWith("_MOVE", StringComparison.Ordinal) ? numberless[..^5] : numberless;
		return new[] { bare, numberless, move }.Where(m => m.Length > 0).Distinct().Select(m => $"{monster}.moves.{m}.title");
	}

	/// <summary>The portrait the art tool renders for an elite (its bestiary pose).</summary>
	public static string PortraitPath(string monster) => $"res://SlaySpoon/images/cards/trophies/{monster.ToLowerInvariant()}.png";

	public static string Stream(int floor, int playerSlot) => $"SlaySpoon.Trophy.{floor}.{playerSlot}";
}
