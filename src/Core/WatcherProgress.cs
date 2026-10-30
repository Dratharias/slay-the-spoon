namespace SlaySpoon.Core;

/// <summary>
/// A Custom run makes the game create a <c>CharacterStats</c> entry for the Watcher in the modded profile. Vanilla
/// epoch checks compare the number of entries with the number of vanilla characters, so her entry must not count.
/// </summary>
public static class WatcherProgress
{
	/// <summary>How many of <paramref name="statIds"/> are vanilla characters (the Watcher and other entries are left out).</summary>
	public static int VanillaStatsCount(IEnumerable<string> statIds, IEnumerable<string> vanillaIds)
	{
		HashSet<string> vanilla = new(vanillaIds);
		return statIds.Count(vanilla.Contains);
	}
}

/// <summary>
/// The Watcher has no ascension progress of her own. A solo run with her may use any ascension the player has
/// unlocked with a vanilla character.
/// </summary>
public static class WatcherAscension
{
	/// <summary>The highest of the vanilla characters' max ascensions, 0 when there are none.</summary>
	public static int Cap(IEnumerable<int> vanillaMaxAscensions) => vanillaMaxAscensions.DefaultIfEmpty(0).Max();
}
