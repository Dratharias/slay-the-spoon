namespace SlaySpoon.Core;

/// <summary>
/// The appended acts' map screen (the Ending, the Spoon) at a height that fits their few rows (owner request, 1.3.1).
/// Vanilla <c>NMapScreen.SetMap</c> spreads any map over the same 2325 px (<c>_distY = 2325 / (rows − 1)</c>) and puts
/// the boss at y = −1980, so a 3-row act was as tall as a 15-room act. Here the rows get the spacing of a solo act 3
/// (Glory, 14 grid rows) and the boss keeps Glory's gap above the last row.
/// </summary>
public static class AppendedMapHeight
{
	private const float VanillaHeight = 2325f, FirstRowY = 740f, VanillaBossY = -1980f;
	private const int ReferenceRows = 14;

	/// <summary>Vanilla scroll limits of the map container (bottom of the map shown, top of the map shown).</summary>
	public const float VanillaScrollBottom = -600f, VanillaScrollTop = 1800f;

	public static float ClampScroll(float position, float top) => Math.Clamp(position, VanillaScrollBottom, top);

	public static float RowSpacing => VanillaHeight / (ReferenceRows - 1);

	/// <summary>Glory's gap between its last grid row and the boss.</summary>
	private static float BossGap => FirstRowY - (ReferenceRows - 1) * RowSpacing - VanillaBossY;

	/// <summary>A point's new y: same row, the new spacing, its jitter (or the start's offset) kept.</summary>
	public static float PointY(float y, int row, float vanillaDistY) => y + row * vanillaDistY - row * RowSpacing;

	public static float BossY(int rowCount) => FirstRowY - (rowCount - 1) * RowSpacing - BossGap;

	/// <summary>Shift the laid-out boss by the height difference, preserving the parent anchor offset.
	/// Vanilla positions the boss before adding it to Points; parenting adds the centered anchor translation.</summary>
	public static float BossPointY(float laidOutY, int rowCount) => laidOutY + BossY(rowCount) - VanillaBossY;

	/// <summary>The top scroll limit: the boss shown where vanilla shows it at the top, never below the bottom limit.</summary>
	public static float ScrollTop(int rowCount) =>
		Math.Max(VanillaScrollBottom, VanillaScrollTop - (BossY(rowCount) - VanillaBossY));
}
