namespace SlaySpoon.Core;

/// <summary>When a run continues into The Ending (act 4).</summary>
public static class EndingRouteRule
{
	/// <summary>The three vanilla acts the Ending is appended to.</summary>
	public const int VanillaActCount = 3;

	/// <summary>
	/// True only for an Act 4 run whose team holds all three keys and that still has exactly the vanilla three acts:
	/// the Ending is appended once and never to a run of another shape.
	/// </summary>
	public static bool ShouldAppend(bool act4Run, bool hasAllKeys, int actCount) =>
		act4Run && hasAllKeys && actCount == VanillaActCount;

	/// <summary>
	/// An appended Ending means the run is not won yet, so a recorded WinTime (act 3 boss counted as final, saved on
	/// its reward screen) is stale: it would freeze the run clock and the Heart would record the time again.
	/// </summary>
	public static bool ShouldResetWinTime(bool justAppended, long winTime) => justAppended && winTime > 0;
}
