using MegaCrit.Sts2.Core.Map;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// The Ending act's map, built from <see cref="EndingMapLayout"/> on vanilla <c>GoldenPathActMap</c>'s pattern: the
/// starting point (the rest site) at row 0 outside the grid's points, the merchant and the elite in the middle
/// column, and the boss one row above the grid. It draws from no Rng, so the run's seeds stay vanilla.
/// </summary>
/// <remarks>
/// The starting point is a rest site, not an ancient: the act has none, and reading <c>act.Ancient</c> would throw.
/// Vanilla already handles a non-ancient start (act 1 without Neow types it Monster): the map screen draws it as a
/// normal point and makes it travelable while nothing is visited. A saved map reloads as a <c>SavedActMap</c> with
/// the same points, links and types.
/// </remarks>
public sealed class EndingActMap : ActMap
{
	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public EndingActMap()
	{
		Grid = new MapPoint?[EndingMapLayout.Width, EndingMapLayout.GridHeight];
		var nodes = EndingMapLayout.Nodes;
		StartingMapPoint = Point(nodes[0]);
		BossMapPoint = Point(nodes[^1]);
		MapPoint previous = StartingMapPoint;
		for (int i = 1; i < nodes.Count - 1; i++)
		{
			MapPoint point = Point(nodes[i]);
			Grid[point.coord.col, point.coord.row] = point;
			previous.AddChildPoint(point);
			if (i == 1) startMapPoints.Add(point);
			previous = point;
		}
		previous.AddChildPoint(BossMapPoint);
	}

	private static MapPoint Point((int Row, EndingNode Node) node) =>
		new(EndingMapLayout.Column, node.Row) { PointType = TypeOf(node.Node) };

	private static MapPointType TypeOf(EndingNode node) => node switch
	{
		EndingNode.RestSite => MapPointType.RestSite,
		EndingNode.Shop => MapPointType.Shop,
		EndingNode.Elite => MapPointType.Elite,
		EndingNode.Boss => MapPointType.Boss,
		_ => throw new ArgumentOutOfRangeException(nameof(node)),
	};
}
