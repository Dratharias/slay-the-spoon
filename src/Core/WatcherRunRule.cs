namespace SlaySpoon.Core;

/// <summary>Solo Standard or Custom Watcher runs use StS1 even without a card-set modifier.</summary>
public static class WatcherRunRule
{
	public static CardSetMode EffectiveMode(CardSetMode configured, bool allowsImplicitMode, int playerCount, bool firstPlayerIsWatcher)
	{
		return configured == CardSetMode.Sts2Only && allowsImplicitMode && playerCount == 1 && firstPlayerIsWatcher
			? CardSetMode.Sts1Only
			: configured;
	}

	/// <summary>
	/// She is an ordinary character (owner, 2026-11-19): offered in Standard and Custom lobbies, solo and co-op, and drawn
	/// by Random there. Daily stays vanilla.
	/// </summary>
	public static bool OfferedInLobby(bool daily) => !daily;
}
