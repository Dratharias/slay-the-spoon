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

	/// <summary>
	/// The recipient slot of each reward from the Conductor's synced picks: one per reward, in order. A missing or
	/// out-of-range pick leaves that reward with its owner, so a short or corrupt list never moves anything by accident.
	/// </summary>
	public static int[] Recipients(IReadOnlyList<int>? picks, IReadOnlyList<int> owners, int players) =>
		owners.Select((owner, i) => picks != null && i < picks.Count && picks[i] >= 0 && picks[i] < players ? picks[i] : owner)
			.ToArray();

	public static string Stream(int floor) => $"SlaySpoon.Conductor.{floor}";

	/// <summary>Most reward columns the screen uses.</summary>
	public const int MaxColumns = 3;

	/// <summary>Portrait columns per reward row: one line for two players, a 2 × 2 square from three.</summary>
	public static int HeadColumns(int players) => players >= 3 ? 2 : Math.Max(1, players);

	/// <summary>
	/// Reward columns for <paramref name="rewards"/> rows of size <paramref name="cellW"/> × <paramref name="cellH"/> on a
	/// <paramref name="viewW"/> × <paramref name="viewH"/> screen: the count whose page needs the least shrinking
	/// (<paramref name="fixedH"/> is the title and button height, <paramref name="gapW"/>/<paramref name="gapH"/> the gaps).
	/// A ratio under 2 % better keeps the fewer columns.
	/// </summary>
	public static int Columns(int rewards, float cellW, float cellH, float fixedH, float gapW, float gapH, float viewW, float viewH)
	{
		int best = 1;
		float bestScale = ScaleFor(1);
		for (int c = 2; c <= Math.Min(MaxColumns, rewards); c++)
		{
			float scale = ScaleFor(c);
			if (scale > bestScale + 0.02f) (best, bestScale) = (c, scale);
		}
		return best;

		float ScaleFor(int c)
		{
			int rows = (rewards + c - 1) / c;
			float w = c * cellW + (c - 1) * gapW + 40;
			float h = rows * cellH + Math.Max(0, rows - 1) * gapH + fixedH + 40;
			return Math.Min(1f, Math.Min(viewW / w, viewH / h));
		}
	}
}
