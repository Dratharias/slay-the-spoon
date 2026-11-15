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

	/// <summary>Gold a rest site action costs: 50, then 50 more for each action already paid at this fire.</summary>
	public const int RestPriceStep = 50;

	public static int RestPrice(int paidHere) => RestPriceStep * (Math.Max(0, paidHere) + 1);
}
