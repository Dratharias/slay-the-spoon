namespace SlaySpoon.Core;

/// <summary>Runtime availability is separate from the original StS1 rarity.</summary>
public static class ColorlessCardRules
{
	/// <summary>Keep Massive Scroll useful when the effective pool has no co-op-exclusive cards.</summary>
	public static IReadOnlyList<T> PreferCoopCards<T>(IReadOnlyList<T> allowed, Func<T, bool> isCoopOnly)
	{
		T[] coop = allowed.Where(isCoopOnly).ToArray();
		return coop.Length > 0 ? coop : allowed;
	}

	public static bool IsEventOnly(string id) => id is
		"STS1_APOTHEOSIS" or "STS1_APPARITION" or "STS1_BITE" or "STS1_ENLIGHTENMENT" or
		"STS1_JAX" or "STS1_METAMORPHOSIS" or "STS1_RITUAL_DAGGER";

	/// <summary>A direct event grant has no ordinary pool slot; select its version explicitly.</summary>
	public static bool UseSts1EventVersion(CardSetMode mode, Sts1Relation relation, bool roll) =>
		relation != Sts1Relation.Retired && (mode == CardSetMode.Sts1Only ||
		(mode == CardSetMode.Hybrid && (relation == Sts1Relation.SameKeepSts1 ||
			(relation == Sts1Relation.Duplicate && roll))));
}
