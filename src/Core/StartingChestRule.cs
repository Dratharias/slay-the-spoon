namespace SlaySpoon.Core;

/// <summary>
/// Starting Chest (Custom Run): at Neow, each player opens a chest of <see cref="Size"/> relics from their own pools and
/// keeps one. Each slot rolls a rarity, then takes the first relic of that rarity not shown yet.
/// </summary>
public static class StartingChestRule
{
	public const int Size = 10;

	/// <summary>
	/// Draws up to <paramref name="count"/> distinct items. <paramref name="next"/> returns the next item for a slot
	/// that is not in the given set yet, or null when nothing is left (the slot and the draw end).
	/// </summary>
	public static List<T> Draw<T>(int count, Func<IReadOnlyCollection<T>, T?> next) where T : class
	{
		var shown = new List<T>(count);
		while (shown.Count < count && next(shown) is { } item)
		{
			if (shown.Contains(item)) throw new InvalidOperationException("next returned an item already shown");
			shown.Add(item);
		}
		return shown;
	}
}
