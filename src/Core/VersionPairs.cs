namespace SlaySpoon.Core;

/// <summary>The two versions of each duplicate card, keyed both ways (StS2 id → StS1 id and StS1 id → StS2 id).</summary>
public static class VersionPairs
{
	public static IReadOnlyDictionary<string, string> Of(IEnumerable<Sts1Entry> entries)
	{
		var pairs = new Dictionary<string, string>();
		foreach (Sts1Entry e in entries.Where(e => e.Relation == Sts1Relation.Duplicate && e.Sts2Id != null))
		{
			pairs[e.Sts2Id!] = e.Sts1Id;
			pairs[e.Sts1Id] = e.Sts2Id!;
		}
		return pairs;
	}
}
