namespace SlaySpoon.Core;

/// <summary>When the shared StS1 potions (Ancient Potion) join the random potion options.</summary>
public static class Sts1PotionRule
{
	/// <summary>Only in StS1-only runs (owner decision, 2026-11-05): Hybrid and the Spire Souvenir keep StS2's potions.</summary>
	public static bool Offered(CardSetMode runMode) => runMode == CardSetMode.Sts1Only;
}
