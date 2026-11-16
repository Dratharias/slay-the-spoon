using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// Halves the map screen's height for the appended acts (<see cref="AppendedMapHeight"/>): after vanilla
/// <c>SetMap</c> lays the points out, each point moves toward the bottom, the dotted paths are drawn again between the
/// new positions (traveled ones recolored as vanilla does), and <c>_distY</c> shrinks so "scroll to the current row"
/// still lands on it. The scroll's top limit shrinks with it. Only the view changes: no map data, seed or save.
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
	private static readonly Action<NMapScreen, NMapPoint, MapPoint> DrawPaths =
		AccessTools.MethodDelegate<Action<NMapScreen, NMapPoint, MapPoint>>(AccessTools.Method(typeof(NMapScreen), "DrawPaths"));

	/// <summary>Vanilla's traveled-path tick scale (<c>_tickTraveledScale</c>).</summary>
	private static readonly Vector2 TraveledScale = Vector2.One * 1.2f;

	private static bool Appended(NMapScreen screen) => Run(screen)?.Act is IAppendedAct;

	[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
	[HarmonyPostfix]
	private static void Shorten(NMapScreen __instance, ActMap map)
	{
		try
		{
			if (!Appended(__instance)) return;
			Dictionary<MapCoord, NMapPoint> points = Points(__instance);
			foreach (NMapPoint node in points.Values.Distinct())
			{
				Vector2 position = node.Position;
				position.Y = AppendedMapHeight.Compress(position.Y);
				node.Position = position;
			}
			DistY(__instance) *= AppendedMapHeight.Factor;

			Paths(__instance).Clear();
			foreach (Node tick in PathsContainer(__instance).GetChildren()) tick.QueueFree();
			foreach (MapPoint point in map.GetAllMapPoints()) DrawPaths(__instance, points[point.coord], point);
			DrawPaths(__instance, points[map.StartingMapPoint.coord], map.StartingMapPoint);
			DrawPaths(__instance, points[map.BossMapPoint.coord], map.BossMapPoint);

			RunState run = Run(__instance);
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

	/// <summary>Vanilla eases a released drag back under 1800; the shorter map stops at <see cref="AppendedMapHeight.ScrollTop"/>.</summary>
	[HarmonyPatch(typeof(NMapScreen), "UpdateScrollPosition")]
	[HarmonyPrefix]
	private static void ClampTop(NMapScreen __instance)
	{
		if (!Appended(__instance)) return;
		ref Vector2 target = ref TargetDragPos(__instance);
		if (target.Y > AppendedMapHeight.ScrollTop) target = new Vector2(target.X, AppendedMapHeight.ScrollTop);
	}
}
