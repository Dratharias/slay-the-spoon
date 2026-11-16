namespace SlaySpoon.Core;

/// <summary>
/// The appended acts' map screen (the Ending, the Spoon) at reduced height (owner request, 1.3.1). Vanilla
/// <c>NMapScreen.SetMap</c> spreads any map over the same 2325 px and puts the boss at y = −1980, so a 3-step act was a
/// long scroll. Every point's y is pulled toward the starting point's line (y = 800, the bottom) by <see cref="Factor"/>.
/// </summary>
public static class AppendedMapHeight
{
	/// <summary>Share of the vanilla height kept.</summary>
	public const float Factor = 0.5f;

	/// <summary>The y vanilla gives a non-ancient starting point at row 0: the line that stays put.</summary>
	public const float Bottom = 800f;

	/// <summary>Vanilla scroll limits of the map container (bottom of the map shown, top of the map shown).</summary>
	public const float VanillaScrollBottom = -600f, VanillaScrollTop = 1800f;

	public static float Compress(float y) => Bottom - (Bottom - y) * Factor;

	/// <summary>The top scroll limit for the shorter map; the bottom limit stays vanilla.</summary>
	public static float ScrollTop => VanillaScrollBottom + (VanillaScrollTop - VanillaScrollBottom) * Factor;
}
