using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Spoon;

namespace SlaySpoon.Act4;

/// <summary>
/// Fits the map screen's height to the appended acts' few rows (<see cref="AppendedMapHeight"/>): after vanilla
/// <c>SetMap</c> lays the points out, each row moves to act 3's spacing and the boss to act 3's gap above the last row,
/// the dotted paths are drawn again between the new positions (traveled ones recolored as vanilla does), and
/// <c>_distY</c> takes the new spacing so "scroll to the current row" still lands on it. The scroll's top limit
/// follows the boss. Only the view changes: no map data, seed or save.
/// </summary>
[HarmonyPatch]
internal static class AppendedMapHeightPatch
{
	private static readonly AccessTools.FieldRef<NMapScreen, RunState> Run =
		AccessTools.FieldRefAccess<NMapScreen, RunState>("_runState");
	private static readonly AccessTools.FieldRef<NMapScreen, Dictionary<MapCoord, NMapPoint>> Points =
		AccessTools.FieldRefAccess<NMapScreen, Dictionary<MapCoord, NMapPoint>>("_mapPointDictionary");
	private static readonly AccessTools.FieldRef<NMapScreen, Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>> Paths =
		AccessTools.FieldRefAccess<NMapScreen, Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>>("_paths");
	private static readonly AccessTools.FieldRef<NMapScreen, Control> PathsContainer =
		AccessTools.FieldRefAccess<NMapScreen, Control>("_pathsContainer");
	private static readonly AccessTools.FieldRef<NMapScreen, float> DistY =
		AccessTools.FieldRefAccess<NMapScreen, float>("_distY");
	private static readonly AccessTools.FieldRef<NMapScreen, Vector2> TargetDragPos =
		AccessTools.FieldRefAccess<NMapScreen, Vector2>("_targetDragPos");
	private static readonly AccessTools.FieldRef<NMapScreen, Control> MapContainer =
		AccessTools.FieldRefAccess<NMapScreen, Control>("_mapContainer");
	private static readonly Action<NMapScreen, NMapPoint, MapPoint> DrawPaths =
		AccessTools.MethodDelegate<Action<NMapScreen, NMapPoint, MapPoint>>(AccessTools.Method(typeof(NMapScreen), "DrawPaths"));

	/// <summary>Vanilla's traveled-path tick scale (<c>_tickTraveledScale</c>).</summary>
	private static readonly Vector2 TraveledScale = Vector2.One * 1.2f;

	/// <summary>The top scroll limit of the map last laid out (one map screen at a time); vanilla for other acts.</summary>
	private static float _scrollTop = AppendedMapHeight.VanillaScrollTop;

	/// <summary>
	/// Keyed on the map itself: on a new run <c>SetMap</c> runs before <c>NMapScreen.Initialize</c> hands the screen
	/// the new run state, so the screen's <c>_runState</c> still held the previous run (in game, 1.3.1: points left at
	/// vanilla height while the scroll was clamped).
	/// </summary>
	private static bool Appended(ActMap map)
	{
		if (map is SpoonActMap or EndingActMap) return true;
		// SavedActMap loses the concrete map type. Use the active manager, since the screen may still
		// reference the previous run during initialization. Identity prevents shortening an unrelated map.
		RunState? run = RunManager.Instance.DebugOnlyGetState();
		return map is SavedActMap && run?.Map == map && run.Act is IAppendedAct;
	}

	[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
	[HarmonyPostfix]
	private static void Shorten(NMapScreen __instance, ActMap map)
	{
		try
		{
			_scrollTop = AppendedMapHeight.VanillaScrollTop;
			if (!Appended(map)) return;
			Dictionary<MapCoord, NMapPoint> points = Points(__instance);
			float vanillaDist = DistY(__instance);
			int rowCount = map.GetRowCount();
			foreach ((MapCoord coord, NMapPoint node) in points)
			{
				Vector2 position = node.Position;
				position.Y = coord == map.BossMapPoint.coord
					? AppendedMapHeight.BossPointY(position.Y, rowCount)
					: AppendedMapHeight.PointY(position.Y, coord.row, vanillaDist);
				node.Position = position;
			}
			DistY(__instance) = AppendedMapHeight.RowSpacing;
			_scrollTop = AppendedMapHeight.ScrollTop(rowCount);

			Paths(__instance).Clear();
			foreach (Node tick in PathsContainer(__instance).GetChildren()) tick.QueueFree();
			foreach (MapPoint point in map.GetAllMapPoints()) DrawPaths(__instance, points[point.coord], point);
			DrawPaths(__instance, points[map.StartingMapPoint.coord], map.StartingMapPoint);
			DrawPaths(__instance, points[map.BossMapPoint.coord], map.BossMapPoint);

			RunState? run = Run(__instance);
			if (run == null || run.Map != map) return;
			IReadOnlyList<MapCoord> visited = run.VisitedMapCoords;
			for (int i = 0; i < visited.Count - 1; i++)
			{
				if (!Paths(__instance).TryGetValue((visited[i], visited[i + 1]), out IReadOnlyList<TextureRect>? ticks)) continue;
				foreach (TextureRect tick in ticks)
				{
					tick.Modulate = run.Act.MapTraveledColor;
					tick.Scale = TraveledScale;
				}
			}
		}
		catch (Exception e)
		{
			SpoonLog.Error($"appended act map height failed: {e.Message}");
		}
	}

	/// <summary>Vanilla eases a released drag back under 1800; the shorter map stops at its own top (<see cref="AppendedMapHeight.ScrollTop"/>).</summary>
	[HarmonyPatch(typeof(NMapScreen), "UpdateScrollPosition")]
	[HarmonyPrefix]
	private static void ClampTop(NMapScreen __instance)
	{
		if (_scrollTop >= AppendedMapHeight.VanillaScrollTop) return; // vanilla acts keep their elastic overscroll
		ref Vector2 target = ref TargetDragPos(__instance);
		target = new Vector2(target.X, AppendedMapHeight.ClampScroll(target.Y, _scrollTop));
		// Opening the map also writes Position directly; clamp it before the vanilla interpolation.
		Control container = MapContainer(__instance);
		container.Position = new Vector2(container.Position.X,
			AppendedMapHeight.ClampScroll(container.Position.Y, _scrollTop));
	}
}
