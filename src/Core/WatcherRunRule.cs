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

	/// <summary>Random draws her only where her button is offered: a solo Standard run.</summary>
	public static bool RandomIncludesWatcher(bool singleplayer, bool standard) => singleplayer && standard;
}
