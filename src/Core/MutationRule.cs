namespace SlaySpoon.Core;

/// <summary>Mutation (Custom Run): when a new act starts, every unupgraded deck card is transformed at random.</summary>
public static class MutationRule
{
	/// <summary>Whether entering act <paramref name="actIndex"/> mutates, given the last act already mutated (−1: none).</summary>
	public static bool Mutates(int actIndex, int lastMutated) => actIndex > 0 && actIndex > lastMutated;

	/// <summary>The RNG stream name for one player's mutation in one act (the run seed is the other half).</summary>
	public static string Stream(int actIndex, int playerSlot) => $"SlaySpoon.Mutation.{actIndex}.{playerSlot}";
}
