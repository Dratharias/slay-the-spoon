namespace SlaySpoon.Core;

/// <summary>Conductor (Custom Run, co-op): who hands out the team's rewards.</summary>
public static class ConductorRule
{
	public static bool Applies(int players) => players >= 2;

	/// <summary>
	/// The Conductor's slot. With a previous Conductor (slot ≥ 0) and two or more players, <paramref name="roll"/> in
	/// [0, players − 2] counts the other players, so nobody conducts twice in a row; otherwise it is in [0, players − 1].
	/// </summary>
	public static int Pick(int players, int previous, int roll)
	{
		if (players <= 1) return 0;
		if (previous < 0 || previous >= players) return Math.Clamp(roll, 0, players - 1);
		int index = Math.Clamp(roll, 0, players - 2);
		return index >= previous ? index + 1 : index;
	}

	/// <summary>The roll range for <see cref="Pick"/>.</summary>
	public static int Rolls(int players, int previous) =>
		players <= 1 ? 1 : previous < 0 || previous >= players ? players : players - 1;

	public static string Stream(int floor) => $"SlaySpoon.Conductor.{floor}";
}
