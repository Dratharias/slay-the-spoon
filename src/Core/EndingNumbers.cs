namespace SlaySpoon.Core;

/// <summary>StS1 Ending numbers by StS2 ascension: 8 Tough, 9 Deadly, 10 Double Boss.</summary>
public static class EndingNumbers
{
	public const int FortifyBlock = 30;
	public const int SpearBurnStrikeHits = 2;
	public const int SpearBurns = 2;
	public const int PiercerStrength = 2;
	public const int DebilitateAmount = 2;

	public static int ShieldHp(int ascension) => ascension >= 8 ? 125 : 110;
	public static int ShieldArtifact(int ascension) => ascension >= 10 ? 2 : 1;
	public static int ShieldBash(int ascension) => ascension >= 9 ? 14 : 12;
	public static int ShieldSmash(int ascension) => ascension >= 9 ? 38 : 34;
	/// <summary>Fixed Smash block from Double Boss; null means the block equals the Smash damage.</summary>
	public static int? ShieldSmashFixedBlock(int ascension) => ascension >= 10 ? 99 : null;

	/// <summary>
	/// StS1 Smash gains Block equal to its hit's damage before Block: a hit that Block absorbed, or that overkilled a
	/// player, counts in full. Co-op takes the highest hit on one player. A10 is the flat fixed block.
	/// </summary>
	public static int ShieldSmashBlock(int ascension, IEnumerable<int> preBlockDamages) =>
		ShieldSmashFixedBlock(ascension) ?? preBlockDamages.DefaultIfEmpty(0).Max();

	public static int SpearHp(int ascension) => ascension >= 8 ? 180 : 160;
	public static int SpearArtifact(int ascension) => ascension >= 10 ? 2 : 1;
	public static int SpearBurnStrike(int ascension) => ascension >= 9 ? 6 : 5;
	public static bool SpearBurnsToDrawPile(int ascension) => ascension >= 10;
	public static int SpearSkewer(int ascension) => 10;
	public static int SpearSkewerHits(int ascension) => ascension >= 9 ? 4 : 3;

	public static int HeartHp(int ascension) => ascension >= 8 ? 800 : 750;
	public static int HeartBeatOfDeath(int ascension) => ascension >= 10 ? 2 : 1;
	public static int HeartBloodShot(int ascension) => 2;
	public static int HeartBloodShotHits(int ascension) => ascension >= 9 ? 15 : 12;
	public static int HeartEcho(int ascension) => ascension >= 9 ? 45 : 40;
}
