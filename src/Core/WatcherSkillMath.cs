namespace SlaySpoon.Core;

/// <summary>Rarity buckets StS1's Foreign Influence rolls for each offered attack.</summary>
public enum ForeignRarity
{
	Common,
	Uncommon,
	Rare,
}

/// <summary>The small numbers behind several Watcher skills.</summary>
public static class WatcherSkillMath
{
	/// <summary>How many cards Scrawl draws to fill a hand of <paramref name="max"/>.</summary>
	public static int CardsToFillHand(int handCount, int max) => Math.Max(0, max - handCount);

	/// <summary>Spirit Shield: <paramref name="perCard"/> Block for each other card in hand.</summary>
	public static int SpiritShieldBlock(int otherCardsInHand, int perCard) => Math.Max(0, otherCardsInHand) * perCard;

	/// <summary>Judgement sets HP to 0 when the enemy has at most <paramref name="cutoff"/> HP.</summary>
	public static bool JudgementKills(int currentHp, int cutoff) => currentHp <= cutoff;

	/// <summary>Foreign Influence rarity for a 0-99 roll: 55 % Common, 30 % Uncommon, 15 % Rare.</summary>
	public static ForeignRarity RarityFromRoll(int roll) => roll < 55 ? ForeignRarity.Common : roll < 85 ? ForeignRarity.Uncommon : ForeignRarity.Rare;

	/// <summary>Collect: the X result, plus one when upgraded; never negative.</summary>
	public static int CollectTurns(int x, bool upgraded) => Math.Max(0, x + (upgraded ? 1 : 0));
}
