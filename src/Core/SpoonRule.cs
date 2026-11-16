namespace SlaySpoon.Core;

/// <summary>
/// Spoon mode (docs/superpowers/specs/2026-11-14-spoon-design.md): numbers and decisions for the Spoon act that
/// follows act 3 once a player reaches 300 max HP.
/// </summary>
public static class SpoonRule
{
	public const int Threshold = 300, FeastMaxHp = 5, GiftMaxHp = 5, HpPerFighter = 2000,
		AppetitePerMove = 2, AppetiteBite = 3, AppetitePercent = 10;

	/// <summary>The fight's numbers (docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md); tuned during balancing.</summary>
	public const int BoucheeDamage = 35, LadleDamage = 55, BanquetMaxHp = 10, FrailAmount = 2, VulnerableAmount = 2,
		BanquetHealBase = 60, BanquetHealPerPlayer = 10, MaxHpPerStrength = 5,
		TeaspoonAppetite = 2, TeaspoonAppetiteUpgraded = 3, SkimmerBlock = 6, SkimmerBlockUpgraded = 9,
		BallerMaxHp = 12, BallerMaxHpUpgraded = 15;

	/// <summary>The 4-turn cycle: sets the table (Frail and the cutlery), Mouthful, Banquet, Ladle Smash.</summary>
	public enum SpoonMove { SetTable, Bouchee, Banquet, Ladle }

	/// <summary>The cutlery one player gets each cycle.</summary>
	public readonly record struct Kit(int Teaspoons, bool Wooden, bool Skimmer, bool Baller)
	{
		public int Count => Teaspoons + (Wooden ? 1 : 0) + (Skimmer ? 1 : 0) + (Baller ? 1 : 0);
	}

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

	/// <summary>Each Appetite stack adds <see cref="AppetitePercent"/> % to the Spoon's damage (owner idea, 2026-11-16).</summary>
	public static decimal AppetiteMultiplier(int stacks) => 1m + Math.Max(0, stacks) * AppetitePercent / 100m;

	/// <summary>The Appetite a Spoonful played on the Spoon removes: <see cref="AppetiteBite"/>, or what is left.</summary>
	public static int AppetiteRemoved(int stacks) => Math.Clamp(stacks, 0, AppetiteBite);

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
	/// minus 1 per as many below, truncated toward zero (owner, 2026-11-16).
	/// </summary>
	public static int StrengthFor(int maxHp, int startMaxHp) => (maxHp - startMaxHp) / MaxHpPerStrength;

	/// <summary>Banquet heals 60, plus 10 per player past the first (owner: 60, 70, 80, 90).</summary>
	public static int BanquetHeal(int players) => BanquetHealBase + BanquetHealPerPlayer * Math.Max(0, players - 1);

	/// <summary>The max HP a Melon Baller takes: its amount, leaving the Spoon at least 1.</summary>
	public static int BallerTake(int spoonMaxHp, int amount = BallerMaxHp) => Math.Clamp(amount, 0, Math.Max(0, spoonMaxHp - 1));

	/// <summary>
	/// The kit by deck size (owner table): ≤ 9 one Teaspoon; 10 + Wooden Spoon; 15 two Teaspoons; 20 + Skimmer;
	/// 25 + Melon Baller; 30 four Teaspoons, then 2 more per further 15 cards (builder call).
	/// </summary>
	public static Kit KitFor(int deckSize)
	{
		int teaspoons = deckSize >= 30 ? 4 + 2 * ((deckSize - 30) / 15) : deckSize >= 15 ? 2 : 1;
		return new Kit(teaspoons, deckSize >= 10, deckSize >= 20, deckSize >= 25);
	}

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
