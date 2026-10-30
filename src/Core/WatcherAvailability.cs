namespace SlaySpoon.Core;

/// <summary>
/// The Watcher is a StS1 character: the Custom Run screen offers her only while the lobby runs a StS1 card set
/// ("Slay the Spire 1 Cards" or "Spire Souvenir"). Ids are ModelDb id entries.
/// </summary>
public static class WatcherAvailability
{
	private static readonly string[] EnablingModifiers =
	{
		ModelIds.FromClassName("SpoonCardSetSts1Only"),
		ModelIds.FromClassName("SpoonSpireSouvenirModifier"),
	};

	/// <summary>True when one of the lobby's modifiers is a StS1 card set.</summary>
	public static bool IsAvailable(IEnumerable<string> modifierIds)
	{
		return modifierIds.Any(id => EnablingModifiers.Contains(id));
	}

	/// <summary>
	/// The character a player keeps after a modifier change: <paramref name="previous"/> (their last vanilla pick) when
	/// they are on the Watcher and she is no longer available, otherwise <paramref name="selected"/>.
	/// </summary>
	public static string Fallback(string selected, string previous, bool available)
	{
		return selected == WatcherAssets.CharacterId && !available ? previous : selected;
	}
}
