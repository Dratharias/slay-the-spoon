namespace SlaySpoon.Core;

/// <summary>
/// Vanilla Neow offers its blessings only when the run has no modifier at all; any modifier makes it offer
/// the modifiers' own Neow options instead (Draft, Sealed Deck…), or nothing. The Neow's Blessings modifier
/// brings the blessings back.
/// </summary>
public static class NeowRule
{
	/// <summary>
	/// The modifier count Neow should see when it picks its branch: zero means blessings.
	/// </summary>
	/// <param name="count">The run's real modifier count.</param>
	/// <param name="blessingTicked">The run has the Neow's Blessings modifier.</param>
	/// <param name="anyNeowOption">A modifier of the run offers its own Neow option.</param>
	/// <param name="forceBlessings">The modifier options are done and the blessings come next.</param>
	public static int VisibleModifierCount(int count, bool blessingTicked, bool anyNeowOption, bool forceBlessings)
	{
		if (!blessingTicked)
		{
			return count;
		}
		// Modifier options keep their vanilla place first; the blessings follow them.
		return forceBlessings || !anyNeowOption ? 0 : count;
	}
}
