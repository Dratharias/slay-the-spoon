namespace SlaySpoon.Core;

/// <summary>
/// Spoon mode (docs/superpowers/specs/2026-11-14-spoon-design.md): numbers and decisions for the Spoon act that
/// follows act 3 once the team's max HP adds up to <see cref="TeamThreshold"/>.
/// </summary>
public static class SpoonRule
{
	public const int FeastMaxHp = 5, HpPerPlayer = 1200,
		AppetitePerMove = 2, AppetiteBite = 3, SpoonfulAppetite = 1, SpoonfulAppetiteUpgraded = 2, AppetitePercent = 5, AppetiteStolenBack = 1;

	/// <summary>The fight's numbers (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md); tuned during balancing.</summary>
	public const int BoucheeDamage = 25, LadleDamage = 35, BanquetDamage = 8, BanquetMaxHp = 10, FrailAmount = 2, VulnerableAmount = 2,
		BanquetHealBase = 30, BanquetHealPerPlayer = 10, MaxHpPerStrength = 5,
		TeaspoonAppetite = 2, TeaspoonAppetiteUpgraded = 3, SkimmerBlock = 6, SkimmerBlockUpgraded = 9,
		BallerMaxHp = 12, BallerMaxHpUpgraded = 15, WoodenMaxHpPercent = 10, ExtraFromDeck = 25, ExtraEveryCards = 5,
		LadleStealDivisor = 4;

	/// <summary>The 4-turn cycle: sets the table (Frail and the cutlery), Mouthful, Banquet, Ladle Smash.</summary>
	public enum SpoonMove { SetTable, Bouchee, Banquet, Ladle }

	/// <summary>The five spoons of the kit; each player gets one of each every cycle (owner, 2026-11-16).</summary>
	/// <remarks>Stolen Spoonful left the kit (owner, 2026-11-16): the deck Spoonful becomes it for the Spoon fight.</remarks>
	public enum Cutlery { Teaspoon, Wooden, Skimmer, Baller }

	/// <summary>
	/// The 12 max HP relics every player starts a Spoon run with (owner, 2026-11-16; they were ordinary draws before), by id
	/// entry, with the max HP their pickup effect gives (0: the relic grows max HP later instead). Removed from every grab bag.
	/// </summary>
	public static readonly IReadOnlyList<(string Entry, int PickupMaxHp)> StartRelics =
	[
		("STRAWBERRY", 7), ("PEAR", 10), ("MANGO", 14), ("FAKE_MANGO", 3), ("LEES_WAFFLE", 7), ("NUTRITIOUS_OYSTER", 11),
		("BIG_MUSHROOM", 20), ("LOOMING_FRUIT", 31), ("DRAGON_FRUIT", 0), ("DARKSTONE_PERIAPT", 0), ("CHOSEN_CHEESE", 0),
		("STONE_HUMIDIFIER", 0),
	];

	/// <summary>Max HP the starting relics add at run start.</summary>
	public static int StartRelicMaxHp => StartRelics.Sum(r => r.PickupMaxHp);

	/// <summary>
	/// The team's summed max HP the Spoon needs (owner, 2026-11-16: no more supporters, everyone fights): 300 solo, then
	/// 550, 750 and 900, so each player needs less as the team grows (kills, and so Spoonful growth, are shared).
	/// </summary>
	private static readonly int[] TeamThresholds = [300, 550, 750, 900];

	public static int TeamThreshold(int players) => TeamThresholds[Math.Clamp(players, 1, TeamThresholds.Length) - 1];

	public static bool ShouldInsert(bool spoonRun, bool vanillaActIndexIsLast3, bool atLastBoss, IReadOnlyList<int> maxHps, bool alreadyInserted) =>
		spoonRun && vanillaActIndexIsLast3 && atLastBoss && !alreadyInserted && maxHps.Count > 0
		&& maxHps.Sum() >= TeamThreshold(maxHps.Count);

	/// <summary>Where the Spoon act goes: after the three vanilla acts, before the Ending when present; -1 for any other act list.</summary>
	public static int InsertIndex(int actCount, bool endingPresent) =>
		(actCount == 3 && !endingPresent) || (actCount == 4 && endingPresent) ? 3 : -1;

	/// <summary>
	/// Each Appetite stack adds <see cref="AppetitePercent"/> % to its holder's damage (owner idea, 2026-11-16). Appetite
	/// can be negative, for the Spoon and the players (owner, 2026-11-16): each negative stack takes 5 % off, down to no
	/// damage. While Sated (Wooden Spoon) the Spoon's Appetite does nothing: its next turn hits at base.
	/// </summary>
	public static decimal AppetiteMultiplier(int stacks, bool sated = false) =>
		sated ? 1m : Math.Max(0m, 1m + stacks * AppetitePercent / 100m);

	/// <summary>The Spoon gains <see cref="AppetitePerMove"/> Appetite only after its turn 1, Sets the Table (owner, 2026-11-16).</summary>
	public static bool GainsAppetite(SpoonMove move) => move == SpoonMove.SetTable;

	/// <summary>
	/// That gain: <see cref="AppetitePerMove"/> per living player, since every player gets the
	/// cutlery that removes it (owner, 2026-11-16: it must grow faster with more players).
	/// </summary>
	public static int AppetiteGain(int livingPlayers) => AppetitePerMove * Math.Max(1, livingPlayers);

	/// <summary>SetTable, Bouchée, Banquet, Ladle, repeating (turn is zero-based).</summary>
	public static SpoonMove Move(int turn) => (SpoonMove)(((turn % 4) + 4) % 4);

	/// <summary>
	/// The Spoon's Strength follows its max HP: 1 per <see cref="MaxHpPerStrength"/> above its max HP at fight start,
	/// minus 1 per as many below, truncated toward zero. All max HP changes count against the fixed combat baseline.
	/// </summary>
	public static int StrengthFor(int maxHp, int startMaxHp) => (maxHp - startMaxHp) / MaxHpPerStrength;

	/// <summary>All unblocked damage becomes max HP (owner retained the full growth rate).</summary>
	public static int GrowthFromDamage(int unblocked) => Math.Max(0, unblocked);

	/// <summary>Banquet heals 30, plus 10 per player past the first.</summary>
	public static int BanquetHeal(int players) => BanquetHealBase + BanquetHealPerPlayer * Math.Max(0, players - 1);

	/// <summary>The max HP a Melon Baller takes: its amount, leaving the Spoon at least 1.</summary>
	public static int BallerTake(int spoonMaxHp, int amount = BallerMaxHp) => Math.Clamp(amount, 0, Math.Max(0, spoonMaxHp - 1));

	/// <summary>Extra spoons per cycle: 1 from a 25-card deck, then 1 more per further 5 cards (owner, 2026-11-16).</summary>
	public static int ExtraSpoons(int deckSize) =>
		deckSize < ExtraFromDeck ? 0 : 1 + (deckSize - ExtraFromDeck) / ExtraEveryCards;

	/// <summary>The kit spoon an extra roll picks.</summary>
	public static Cutlery ExtraPick(int roll)
	{
		int count = Enum.GetValues<Cutlery>().Length;
		return (Cutlery)(((roll % count) + count) % count);
	}

	/// <summary>The Wooden Spoon's damage: 10 % of the player's max HP, rounded down (owner, 2026-11-16).</summary>
	public static int WoodenDamage(int maxHp) => Math.Max(0, maxHp) * WoodenMaxHpPercent / 100;

	/// <summary>
	/// The Appetite the Spoon takes from a player hit by an attack, even if fully blocked (owner, 2026-11-16); the
	/// player may go negative.
	/// </summary>
	public static int StolenBack(bool hit) => hit ? AppetiteStolenBack : 0;

	/// <summary>
	/// Ordinary monsters in a Spoon run (owner, 2026-11-16): each hit that reaches a player, even fully blocked, takes
	/// <see cref="AppetiteStolenBack"/> Appetite from them and gives it to the attacker. The Spoon keeps its per-move rule.
	/// </summary>
	public static int StolenOnHits(int hits) => Math.Max(0, hits) * AppetiteStolenBack;

	/// <summary>
	/// Ladle Smash steals max HP (owner, 2026-11-16): a quarter of the damage it dealt past a player's Block, rounded
	/// down, leaving them at least 1 max HP. Blocking it all steals nothing.
	/// </summary>
	public static int LadleMaxHpSteal(int unblocked, int playerMaxHp) =>
		Math.Clamp(Math.Max(0, unblocked) / LadleStealDivisor, 0, Math.Max(0, playerMaxHp - 1));

	public static int JuiceSlot(int potionCount, bool hasJuice, int roll) =>
		hasJuice || potionCount <= 0 ? -1 : ((roll % potionCount) + potionCount) % potionCount;
}
