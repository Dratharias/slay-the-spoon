namespace SlaySpoon.Core;

/// <summary>
/// The "Selected: X / Y" line on multi-pick card grids (owner decision 2026-11-05: every grid, vanilla modes included,
/// display only). Pure text logic; <c>SelectionCounterPatch</c> applies it.
/// </summary>
public static class SelectionCounter
{
	/// <summary>The pick limit shown: the prefs' maximum, capped by the cards offered; null when no counter is shown.</summary>
	public static int? Limit(int maxSelect, int cardCount)
	{
		int limit = Math.Min(maxSelect, cardCount);
		return limit > 1 ? limit : null;
	}

	/// <summary>
	/// Appends <paramref name="counter"/> to the screen's prompt on a new line, inside a trailing <c>[center]</c> block
	/// when the prompt ends with one so the counter stays centred with it.
	/// </summary>
	public static string Compose(string prompt, string counter)
	{
		const string close = "[/center]";
		if (prompt.EndsWith(close, StringComparison.Ordinal))
			return prompt[..^close.Length] + "\n" + counter + close;
		return prompt + "\n" + counter;
	}
}
