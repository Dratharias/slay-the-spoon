using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon act's map: the starting point (a rest site) at row 0, outside the grid's points, linked straight to the
/// boss one row above the grid. It draws from no Rng.
/// </summary>
/// <remarks>
/// The grid has two empty rows, not one: <c>NMapScreen.SetMap</c> spaces rows by <c>2325 / (rowCount − 1)</c>, so a
/// one-row grid divides by zero (and the start's position, row 0 × −∞, is NaN). The empty rows hold no points, so
/// nothing is drawn there; the start's only child is the boss. A saved map reloads as a <c>SavedActMap</c> with the
/// same points and links.
/// </remarks>
public sealed class SpoonActMap : ActMap
{
	private const int GridHeight = 2;

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public SpoonActMap()
	{
		Grid = new MapPoint?[EndingMapLayout.Width, GridHeight];
		StartingMapPoint = new MapPoint(EndingMapLayout.Column, 0) { PointType = MapPointType.RestSite };
		BossMapPoint = new MapPoint(EndingMapLayout.Column, GridHeight) { PointType = MapPointType.Boss };
		StartingMapPoint.AddChildPoint(BossMapPoint);
	}
}

/// <summary>
/// Free travel (Winged Boots, the Flight modifier) offers the next grid row instead of the point's children, and the
/// Spoon map's next row is empty: the boss would be unreachable from the start. In the Spoon act a point's children
/// are always travelable; the boss is the start's only child, so nothing else changes.
/// </summary>
[HarmonyPatch(typeof(MapTravel), nameof(MapTravel.GetTravelablePointsFrom))]
internal static class SpoonMapTravelPatch
{
	private static void Postfix(IRunState runState, MapPoint currentPoint, ref IEnumerable<MapPoint> __result)
	{
		if (runState.Act is SpoonAct) __result = currentPoint.Children;
	}
}
