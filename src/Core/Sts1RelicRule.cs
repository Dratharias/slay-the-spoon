namespace SlaySpoon.Core;

/// <summary>Rules shared by the relic ports and their game-independent tests.</summary>
public static class Sts1RelicRule
{
	public static bool Offered(bool custom, bool sts1Cards, bool souvenirOption) => custom && (sts1Cards || souvenirOption);
	public static decimal CombatHeal(decimal amount) => amount <= 0 ? amount : Math.Round(amount * 1.5m, MidpointRounding.AwayFromZero);
	public static bool WristBlade(bool attack, bool costsX, int currentCost) => attack && !costsX && currentCost == 0;
	public static int InserterCounter(int previous) => (previous + 1) % 2;

	/// <summary>Busted Crown: the card reward options left, never fewer than one (StS1 keeps at least one).</summary>
	public static int BustedCrownOptions(int offered, int removed) => offered <= 0 ? offered : Math.Max(1, offered - removed);

	/// <summary>Sacred Bark: the factor on every numeric var of a potion its holder carries.</summary>
	public static int PotionFactor(bool holdsBark) => holdsBark ? 2 : 1;
}
