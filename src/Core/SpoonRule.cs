namespace SlaySpoon.Core;

/// <summary>
/// Spoon mode (docs/superpowers/specs/2026-11-14-spoon-design.md): numbers and decisions for the Spoon act that
/// follows act 3 once a player reaches 300 max HP.
/// </summary>
public static class SpoonRule
{
	public const int Threshold = 300, FeastMaxHp = 5, GiftMaxHp = 5, HpPerFighter = 2000,
		AppetitePerMove = 2, AppetiteBite = 3, SpoonfulAppetite = 1, SpoonfulAppetiteUpgraded = 2, AppetitePercent = 5, AppetiteStolenBack = 1;

	/// <summary>The fight's numbers (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md); tuned during balancing.</summary>
	public const int BoucheeDamage = 35, LadleDamage = 55, BanquetDamage = 12, BanquetMaxHp = 10, FrailAmount = 2, VulnerableAmount = 2,
		BanquetHealBase = 60, BanquetHealPerPlayer = 10, MaxHpPerStrength = 5,
		TeaspoonAppetite = 2, TeaspoonAppetiteUpgraded = 3, SkimmerBlock = 6, SkimmerBlockUpgraded = 9,
		BallerMaxHp = 12, BallerMaxHpUpgraded = 15, WoodenMaxHpPercent = 10, ExtraFromDeck = 25, ExtraEveryCards = 5,
		LadleStealDivisor = 4;

	/// <summary>The 4-turn cycle: sets the table (Frail and the cutlery), Mouthful, Banquet, Ladle Smash.</summary>
	public enum SpoonMove { SetTable, Bouchee, Banquet, Ladle }

	/// <summary>The five spoons of the kit; each player gets one of each every cycle (owner, 2026-11-16).</summary>
	public enum Cutlery { Teaspoon, Wooden, Skimmer, Baller, StolenSpoonful }

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
	/// That gain: <see cref="AppetitePerMove"/> per living player, supporters included, since every player gets the
	/// cutlery that removes it (owner, 2026-11-16: it must grow faster with more players).
	/// </summary>
	public static int AppetiteGain(int livingPlayers) => AppetitePerMove * Math.Max(1, livingPlayers);

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

	/// <summary>SetTable, Bouchée, Banquet, Ladle, repeating (turn is zero-based).</summary>
	public static SpoonMove Move(int turn) => (SpoonMove)(((turn % 4) + 4) % 4);

	/// <summary>
	/// The Spoon's Strength follows its max HP: 1 per <see cref="MaxHpPerStrength"/> above its max HP at fight start,
	/// minus 1 per as many below, truncated toward zero (owner, 2026-11-16). The max HP it grew from its hits does not
	/// count (owner, 2026-11-16: growth fed Strength, which fed growth, up to 200+ hits). What it loses (Melon Baller)
	/// and the max HP Ladle Smash steals (1 Strength per 5 stolen, owner, 2026-11-16) do.
	/// </summary>
	public static int StrengthFor(int maxHp, int startMaxHp, int grown = 0) => (maxHp - grown - startMaxHp) / MaxHpPerStrength;

	/// <summary>Banquet heals 60, plus 10 per player past the first (owner: 60, 70, 80, 90).</summary>
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
	/// The Appetite the Spoon takes from a player after a move whose hits pierced their Block (owner, 2026-11-16); the
	/// player may go negative.
	/// </summary>
	public static int StolenBack(int unblocked) => unblocked <= 0 ? 0 : AppetiteStolenBack;

	/// <summary>
	/// Ladle Smash steals max HP (owner, 2026-11-16): a quarter of the damage it dealt past a player's Block, rounded
	/// down, leaving them at least 1 max HP. Blocking it all steals nothing.
	/// </summary>
	public static int LadleMaxHpSteal(int unblocked, int playerMaxHp) =>
		Math.Clamp(Math.Max(0, unblocked) / LadleStealDivisor, 0, Math.Max(0, playerMaxHp - 1));

	/// <summary>
	/// Spoon-Feed is playable while the giver has more than 1 max HP (owner decision 2026-11-16): it gives down to 1 max HP.
	/// </summary>
	public static bool CanGift(int ownerMaxHp) => ownerMaxHp > 1;

	/// <summary>
	/// The max HP Spoon-Feed takes from the giver: <see cref="GiftMaxHp"/>, or less so the giver keeps 1 max HP. Vanilla
	/// LoseMaxHp then damages the giver down to the new max HP at most, which is at least 1, so the gift is never lethal.
	/// </summary>
	public static int GiftAmount(int ownerMaxHp) => Math.Clamp(ownerMaxHp - 1, 0, GiftMaxHp);

	/// <summary>The max HP a fighter gains from Spoon-Feed: what the giver actually lost, never more than the gift.</summary>
	public static int GiftTransferred(int ownerMaxHpBefore, int ownerMaxHpAfter) =>
		Math.Clamp(ownerMaxHpBefore - ownerMaxHpAfter, 0, GiftMaxHp);

	public static int JuiceSlot(int potionCount, bool hasJuice, int roll) =>
		hasJuice || potionCount <= 0 ? -1 : ((roll % potionCount) + potionCount) % potionCount;
}
