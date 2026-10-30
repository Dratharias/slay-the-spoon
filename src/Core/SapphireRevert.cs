namespace SlaySpoon.Core;

public sealed record PickupSnapshot(IReadOnlyDictionary<int, string> Deck, int Gold, int MaxHp, int PotionCount);
public sealed record PickupRevertPlan(bool Reversible, IReadOnlyList<int> Restore, IReadOnlyList<int> Remove,
	int GoldToLose, int MaxHpToLose, int PotionsToDiscard);

/// <summary>Computes only the changes caused by a chest relic's pickup, using stable card handles.</summary>
public static class SapphireRevert
{
	public static PickupRevertPlan Plan(PickupSnapshot before, PickupSnapshot after) => new(
		before.Deck.Keys.All(after.Deck.ContainsKey),
		before.Deck.Keys.Where(k => after.Deck.TryGetValue(k, out string? card) && card != before.Deck[k]).Order().ToArray(),
		after.Deck.Keys.Where(k => !before.Deck.ContainsKey(k)).Order().ToArray(),
		Math.Max(0, after.Gold - before.Gold), Math.Max(0, after.MaxHp - before.MaxHp),
		Math.Max(0, after.PotionCount - before.PotionCount));
}
