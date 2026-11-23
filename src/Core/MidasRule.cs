namespace SlaySpoon.Core;

/// <summary>
/// King Midas (docs/superpowers/specs/2026-11-14-roi-midas-design.md): which events a "?" room may draw and which of
/// their options remain. Events are named by their localization prefix (the model id entry); options by text key.
/// </summary>
public static class MidasRule
{
	/// <summary>The gold events, vanilla then StS1, by id entry.</summary>
	public static readonly IReadOnlyList<string> GoldEvents =
	[
		"LOST_WISP", "THE_LANTERN_KEY", "DENSE_VEGETATION", "COLOSSAL_FLOWER", "JUNGLE_MAZE_ADVENTURE", "SUNKEN_STATUE",
		"THIS_OR_THAT", "SUNKEN_TREASURY", "TRASH_HEAP", "STS1_MIND_BLOOM", "STS1_NEST", "STS1_MOAI_HEAD",
	];

	/// <summary>Option text-key prefixes that lead to gold. Any key of a listed event that matches none is removed.</summary>
	public static readonly IReadOnlyList<string> GoldOptionPrefixes =
	[
		"LOST_WISP.pages.INITIAL.options.SEARCH",
		"THE_LANTERN_KEY.pages.INITIAL.options.RETURN_THE_KEY",
		"DENSE_VEGETATION.pages.INITIAL.options.TRUDGE_ON",
		"COLOSSAL_FLOWER.pages.INITIAL.options.",
		"COLOSSAL_FLOWER.pages.REACH_DEEPER_1.options.",
		"COLOSSAL_FLOWER.pages.REACH_DEEPER_2.options.EXTRACT_INSTEAD",
		"JUNGLE_MAZE_ADVENTURE.pages.INITIAL.options.",
		"SUNKEN_STATUE.pages.INITIAL.options.DIVE_INTO_WATER",
		"THIS_OR_THAT.pages.INITIAL.options.PLAIN",
		"SUNKEN_TREASURY.pages.INITIAL.options.",
		"TRASH_HEAP.pages.INITIAL.options.GRAB",
		"STS1_MIND_BLOOM.pages.INITIAL.options.RICH",
		"STS1_NEST.pages.INITIAL.options.SMASH_AND_GRAB",
		"STS1_MOAI_HEAD.pages.INITIAL.options.OFFER",
	];

	/// <summary>
	/// Indexes of the options to keep. A page of a gold event keeps its enabled gold options; a page with none (or an
	/// option list outside the gold events) is returned whole, so no event is ever left without a way out.
	/// </summary>
	public static IEnumerable<int> KeptOptions(IReadOnlyList<string> keys, IReadOnlyList<bool> enabled)
	{
		var all = Enumerable.Range(0, keys.Count).ToList();
		if (keys.Count == 0 || !keys.All(k => GoldEvents.Any(e => k.StartsWith(e + ".pages.", StringComparison.Ordinal))))
			return all;
		var gold = all.Where(i => enabled[i] && IsGold(keys[i])).ToList();
		return gold.Count > 0 ? gold : all;
	}

	private static bool IsGold(string key) =>
		GoldOptionPrefixes.Any(p => key.StartsWith(p, StringComparison.Ordinal) && !key.EndsWith("_LOCKED", StringComparison.Ordinal));

	/// <summary>Index into <paramref name="count"/> candidates for a random draw <paramref name="roll"/> ≥ 0; -1 when there are none.</summary>
	public static int Pick(int count, int roll) => count <= 0 ? -1 : roll % count;

	/// <summary>
	/// The price scale: 2 when vanilla Midas (gold rewards ×2) is ticked too, else 1. It doubles the melt's share and
	/// minimum, the merchant's price rise and the rest site prices, so doubled income meets doubled costs (the melt's
	/// income part already follows the doubled rewards). Owner request, 2026-11-23.
	/// </summary>
	public static int Scale(bool vanillaMidas) => vanillaMidas ? 2 : 1;

	/// <summary>Gold a rest site action costs: 50, then 50 more for each action already paid at this fire (× scale).</summary>
	public const int RestPriceStep = 50;

	public static int RestPrice(int paidHere, int scale = 1) => RestPriceStep * scale * (Math.Max(0, paidHere) + 1);

	/// <summary>Share of a player's gold that melts each time the team travels to a map room, in percent.</summary>
	public const int MeltPercent = 10;

	/// <summary>The least the fixed part of the melt can be.</summary>
	public const int MeltMinimum = 25;

	/// <summary>The fixed part as a share of the player's average gold gained per floor, in percent.</summary>
	public const int MeltIncomePercent = 100;

	/// <summary>
	/// The fixed part of the melt: the average gold the player gained per floor so far this run (combat, events, relics),
	/// at least 25. Ordinary income only just covers it, so gold kept is gold slowly lost, and a run that earns more
	/// (rich events, gold relics) must keep a bigger reserve to survive the next floor.
	/// </summary>
	public static int MeltFloor(int goldGained, int floors, int scale = 1) =>
		floors <= 0 ? MeltMinimum * scale
			: Math.Max(MeltMinimum * scale, Math.Max(goldGained, 0) / floors * MeltIncomePercent / 100);

	/// <summary>Gold lost on traveling to the next floor: 10 % of the purse, at least <paramref name="floor"/>, never more than the purse.</summary>
	public static int Melt(int gold, int floor, int scale = 1) =>
		Math.Min(Math.Max(gold, 0), Math.Max(gold * MeltPercent * scale / 100, floor));

	/// <summary>True when the next melt leaves no gold: the player dies on traveling.</summary>
	public static bool Ruined(int gold, int floor, int scale = 1) => gold - Melt(gold, floor, scale) <= 0;

	/// <summary>Gold a player already dead on traveling (ruined by an earlier melt) gains instead of paying.</summary>
	public const int RevivalGift = 1;

	/// <summary>What a travel does to one player.</summary>
	public enum Travel { Pay, PayAndDie, Revive }

	/// <summary>
	/// A dead player pays nothing, gains <see cref="RevivalGift"/> and comes back to life for the room (owner, 2026-11-23);
	/// a living one pays the melt and dies if it empties the purse. So a broke player alternates: dead in one room, alive
	/// with 1 gold in the next, and any gold that room gives can pull them out.
	/// </summary>
	public static Travel OnTravel(bool dead, int gold, int floor, int scale = 1) =>
		dead ? Travel.Revive : Ruined(gold, floor, scale) ? Travel.PayAndDie : Travel.Pay;

	/// <summary>
	/// True when the player may pay <paramref name="price"/>: they hold it and keep enough to survive the next melt.
	/// The merchant and the rest site refuse what would kill.
	/// </summary>
	public static bool CanSpend(int gold, int price, int floor, int scale = 1) =>
		gold >= price && !Ruined(gold - price, floor, scale);

	/// <summary>Merchant price rise per item already bought at this merchant, in percent of the price.</summary>
	public const int PriceRisePercent = 10;

	/// <summary>Price multiplier after <paramref name="boughtHere"/> purchases at the current merchant.</summary>
	public static decimal PriceFactor(int boughtHere, int scale = 1) =>
		1m + PriceRisePercent * scale * Math.Max(0, boughtHere) / 100m;

	/// <summary>True when the player can pay for none of the rest site's options: they may only leave.</summary>
	public static bool LeaveOnly(IEnumerable<bool> affordable) => !affordable.Any(a => a);
}
