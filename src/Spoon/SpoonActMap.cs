using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The Spoon act's map: the starting point (a rest site) at row 0, outside the grid's points, a merchant on grid row 1,
/// then the boss one row above the grid. It draws from no Rng.
/// </summary>
/// <remarks>
/// <c>NMapScreen.SetMap</c> always spreads the rows over the same height (<c>2325 / (rowCount − 1)</c>) and draws the
/// boss at a fixed height, so a short act is a long walk. Three grid rows put the merchant (row 1) near halfway
/// between the start and the boss, as the Ending's merchant; row 2 stays empty. A saved map reloads as a
/// <c>SavedActMap</c> with the same points and links.
/// </remarks>
public sealed class SpoonActMap : ActMap
{
	private const int GridHeight = 3;
	private const int MerchantRow = 1;

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid { get; }

	public SpoonActMap()
	{
		Grid = new MapPoint?[EndingMapLayout.Width, GridHeight];
		StartingMapPoint = new MapPoint(EndingMapLayout.Column, 0) { PointType = MapPointType.RestSite };
		BossMapPoint = new MapPoint(EndingMapLayout.Column, GridHeight) { PointType = MapPointType.Boss };
		var merchant = new MapPoint(EndingMapLayout.Column, MerchantRow) { PointType = MapPointType.Shop };
		Grid[merchant.coord.col, merchant.coord.row] = merchant;
		startMapPoints.Add(merchant);
		StartingMapPoint.AddChildPoint(merchant);
		merchant.AddChildPoint(BossMapPoint);
	}
}

/// <summary>
/// Free travel (Winged Boots, the Flight modifier) offers the next grid row instead of the point's children, and the
/// Spoon map's row 2 is empty: the boss would be unreachable from the merchant. In the Spoon act a point's children
/// are always travelable; each point has one child (start → merchant → boss), so nothing else changes.
/// </summary>
[HarmonyPatch(typeof(MapTravel), nameof(MapTravel.GetTravelablePointsFrom))]
internal static class SpoonMapTravelPatch
{
	private static void Postfix(IRunState runState, MapPoint currentPoint, ref IEnumerable<MapPoint> __result)
	{
		if (runState.Act is SpoonAct) __result = currentPoint.Children;
	}
}
