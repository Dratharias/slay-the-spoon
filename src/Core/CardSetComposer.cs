namespace SlaySpoon.Core;

/// <summary>A StS1 card class of one character, and the StS2 card it claims (null when unique).</summary>
public sealed record Sts1Entry(string Sts1Id, string? Sts2Id, Sts1Relation Relation);

/// <summary>
/// Builds a character's card pool for a card set mode, as model ids. The output order depends only on the
/// order of <c>vanilla</c> and <c>sts1</c>: co-op clients roll the same RNG on it.
/// </summary>
public static class CardSetComposer
{
	/// <param name="vanilla">The character's vanilla pool, in the game's order.</param>
	/// <param name="matchedSts2">Vanilla ids that have a StS1 namesake (only membership is read).</param>
	/// <param name="sts1">The character's StS1 card classes, in a stable order.</param>
	public static IReadOnlyList<string> Compose(CardSetMode mode, IReadOnlyList<string> vanilla,
		IReadOnlyCollection<string> matchedSts2, IReadOnlyList<Sts1Entry> sts1)
	{
		switch (mode)
		{
			case CardSetMode.Sts1Only when sts1.Count > 0:
			{
				// A namesake plays as its vanilla card unless a StS1 class claims it.
				HashSet<string?> claimed = sts1.Select(e => e.Sts2Id).ToHashSet();
				return vanilla.Where(id => matchedSts2.Contains(id) && !claimed.Contains(id))
					.Concat(sts1.Select(e => e.Sts1Id))
					.ToList();
			}
			case CardSetMode.Hybrid:
			{
				HashSet<string?> replaced = sts1.Where(e => e.Relation == Sts1Relation.SameKeepSts1)
					.Select(e => e.Sts2Id).ToHashSet();
				return vanilla.Where(id => !replaced.Contains(id))
					.Concat(sts1.Select(e => e.Sts1Id))
					.ToList();
			}
			default:
				return vanilla.ToList();
		}
	}
}
