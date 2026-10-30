namespace SlaySpoon.Core;

/// <summary>The only room kinds the Ending act can hold: it has no events, normal fights, chests or ancient.</summary>
public enum EndingNode { RestSite, Shop, Elite, Boss }

/// <summary>
/// StS1's act 4 map: one straight path, rest site → merchant → elite → Heart, in the middle column.
/// </summary>
/// <remarks>
/// As in vanilla StS2 maps, the first node is the map's starting point (row 0, outside the grid's points) and the
/// boss sits one row above the grid. The nodes in between fill grid rows 1 to <see cref="GridHeight"/> − 1.
/// </remarks>
public static class EndingMapLayout
{
	/// <summary>Grid width, as every vanilla map (7 columns).</summary>
	public const int Width = 7;

	/// <summary>The middle column, where vanilla puts the starting point and the boss.</summary>
	public const int Column = Width / 2;

	public static IReadOnlyList<(int Row, EndingNode Node)> Nodes { get; } =
		[(0, EndingNode.RestSite), (1, EndingNode.Shop), (2, EndingNode.Elite), (3, EndingNode.Boss)];

	/// <summary>Rows the path spans, boss included.</summary>
	public static int Rows => Nodes.Count;

	/// <summary>Height of the map grid: every row but the boss's.</summary>
	public static int GridHeight => Rows - 1;
}
