namespace SlaySpoon.Core;

/// <summary>
/// Spoon mode (docs/superpowers/specs/2026-11-14-spoon-design.md): numbers and decisions for the Spoon act that
/// follows act 3 once a player reaches 300 max HP.
/// </summary>
public static class SpoonRule
{
	public const int Threshold = 300, FeastMaxHp = 5, GiftMaxHp = 5, HpPerFighter = 1500, BoucheeDamage = 30,
		BoucheeMaxHp = 10, LadleDamage = 50, StrengthPerTurn = 3;

	public enum SpoonMove { Bouchee, Ladle }

	/// <summary>Event and Ancient relics that join the ordinary deques, by id entry, with the deque name.</summary>
	public static readonly IReadOnlyList<(string Entry, string Deque)> ExtraRelics =
	[
		("LOOMING_FRUIT", "Rare"), ("BIG_MUSHROOM", "Rare"), ("NUTRITIOUS_OYSTER", "Uncommon"),
		("STONE_HUMIDIFIER", "Uncommon"), ("DARKSTONE_PERIAPT", "Uncommon"), ("CHOSEN_CHEESE", "Uncommon"),
		("FAKE_MANGO", "Common"),
	];

	public static bool ShouldInsert(bool spoonRun, bool vanillaActIndexIsLast3, bool atLastBoss, IReadOnlyList<int> maxHps, bool alreadyInserted) =>
		spoonRun && vanillaActIndexIsLast3 && atLastBoss && !alreadyInserted && maxHps.Any(h => h >= Threshold);

	/// <summary>Where the Spoon act goes: after the three vanilla acts, before the Ending when present; -1 for any other act list.</summary>
	public static int InsertIndex(int actCount, bool endingPresent) =>
		(actCount == 3 && !endingPresent) || (actCount == 4 && endingPresent) ? 3 : -1;

	public static IReadOnlyList<int> Fighters(IReadOnlyList<int> maxHps) =>
		Enumerable.Range(0, maxHps.Count).Where(i => maxHps[i] >= Threshold).ToList();

	public static decimal DamageMultiplier(int players, int teamMaxHp) =>
		teamMaxHp <= 0 || players <= 0 ? 1m : (decimal)Threshold * players / teamMaxHp;

	public static int ScaledDamage(int baseDamage, int players, int teamMaxHp)
	{
		if (baseDamage <= 0) return 0;
		if (teamMaxHp <= 0 || players <= 0) return baseDamage;
		return (int)Math.Max(1L, (long)baseDamage * Threshold * players / teamMaxHp);
	}

	/// <summary>
	/// Multiplier for the running damage amount in a multiplicative damage hook, so that the game's truncation of
	/// <c>amount × factor</c> gives <see cref="ScaledDamage"/> for an integer amount (at least 1 when amount is positive).
	/// Decimal division can land a hair under an exact integer product; the nudge is far below the smallest real
	/// fractional step (1 / teamMaxHp), so it never lifts a non-integer product past the next integer.
	/// </summary>
	public static decimal DamageFactor(decimal amount, int players, int teamMaxHp)
	{
		if (amount <= 0m) return 1m;
		decimal factor = DamageMultiplier(players, teamMaxHp) + FactorNudge;
		return amount * factor < 1m ? 1m / amount + FactorNudge : factor;
	}

	private const decimal FactorNudge = 0.00000000000000000001m;

	/// <summary>
	/// The fighters of a Spoon fight: the players at <see cref="Threshold"/>+ max HP, or every player when none qualifies
	/// (a fight started without the act's insertion rule, such as from the console), so the boss never has 0 HP.
	/// </summary>
	public static IReadOnlyList<int> FightersAtFight(IReadOnlyList<int> maxHps)
	{
		IReadOnlyList<int> fighters = Fighters(maxHps);
		return fighters.Count > 0 ? fighters : Enumerable.Range(0, maxHps.Count).ToList();
	}

	public static int BossHp(int fighters, int players) =>
		players <= 0 ? HpPerFighter : (HpPerFighter * fighters + players - 1) / players;

	/// <summary>Bouchée, Bouchée, Ladle, repeating (turn is zero-based).</summary>
	public static SpoonMove Move(int turn) => turn % 3 == 2 ? SpoonMove.Ladle : SpoonMove.Bouchee;

	public static int JuiceSlot(int potionCount, bool hasJuice, int roll) =>
		hasJuice || potionCount <= 0 ? -1 : ((roll % potionCount) + potionCount) % potionCount;
}
