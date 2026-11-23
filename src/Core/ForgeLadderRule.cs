namespace SlaySpoon.Core;

/// <summary>Which levels the Forge ladder banner lists (spec docs/superpowers/specs/2026-11-23-forge-ladder-banner-design.md).</summary>
public static class ForgeLadderRule
{
	public const int Columns = 3;

	/// <summary>Shown only with "View Upgrade" ticked, in a Forge run, for a card fusion takes past +1, below its limit.</summary>
	public static bool Shows(bool ticked, bool forgeRun, int limit, int level) =>
		ticked && forgeRun && limit > 1 && level < limit;

	/// <summary>The levels from the next one up to the limit.</summary>
	public static IReadOnlyList<int> Levels(int level, int limit)
	{
		int from = Math.Max(level, 0) + 1;
		return from > limit ? [] : Enumerable.Range(from, limit - from + 1).ToList();
	}

	/// <summary>Grid cell of the i-th level: (column, row).</summary>
	public static (int Column, int Row) Cell(int index) => (index % Columns, index / Columns);
}
