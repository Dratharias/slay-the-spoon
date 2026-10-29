namespace SlaySpoon.Core;

/// <summary>
/// The Spire Souvenir relic puts its owner, and only its owner, in Hybrid: StS1 cards join a StS2 run, StS2 cards
/// join a StS1-only run (relic spec §2).
/// </summary>
public static class SouvenirRule
{
	/// <summary>The card set mode one player's card sources follow.</summary>
	public static CardSetMode EffectiveMode(CardSetMode runMode, bool ownsSouvenir)
	{
		return ownsSouvenir ? CardSetMode.Hybrid : runMode;
	}

	/// <summary>The relic may drop: it does nothing in a Hybrid run, and Daily runs stay vanilla.</summary>
	public static bool IsOffered(CardSetMode runMode, bool isDaily)
	{
		return runMode != CardSetMode.Hybrid && !isDaily;
	}
}
