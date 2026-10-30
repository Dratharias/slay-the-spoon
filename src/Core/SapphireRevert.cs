namespace SlaySpoon.Core;

public sealed record PickupSnapshot(IReadOnlyDictionary<int, string> Deck, int Gold, int MaxHp, int PotionCount,
	int CurrentHp, int MaxPotionCount);
public sealed record PickupRevertPlan(bool Reversible, IReadOnlyList<int> Restore, IReadOnlyList<int> Remove,
	int GoldToLose, int MaxHpToLose, int PotionsToDiscard, int HpToLose, int PotionSlotsToLose);

/// <summary>Computes only the changes caused by a chest relic's pickup, using stable card handles.</summary>
public static class SapphireRevert
{
	public static PickupRevertPlan Plan(PickupSnapshot before, PickupSnapshot after) => new(
		before.Deck.Keys.All(after.Deck.ContainsKey),
		before.Deck.Keys.Where(k => after.Deck.TryGetValue(k, out string? card) && card != before.Deck[k]).Order().ToArray(),
		after.Deck.Keys.Where(k => !before.Deck.ContainsKey(k)).Order().ToArray(),
		Math.Max(0, after.Gold - before.Gold), Math.Max(0, after.MaxHp - before.MaxHp),
		Math.Max(0, after.PotionCount - before.PotionCount),
		Math.Max(0, Math.Min(after.CurrentHp - before.CurrentHp, after.CurrentHp - 1)),
		Math.Max(0, after.MaxPotionCount - before.MaxPotionCount));

	/// <summary>
	/// Current HP still to take after the max HP loss: losing max HP already clamps current HP, so that part is not
	/// taken twice. Never drops the player below 1 HP and never heals.
	/// </summary>
	public static int HpLoss(int planned, int alreadyLost, int currentHp) =>
		Math.Max(0, Math.Min(planned - alreadyLost, currentHp - 1));

	/// <summary>
	/// Decides a queued swap from run state only, so every peer reaches the same answer before the key is taken.
	/// Returns the recorded plan, or null when the swap must not happen.
	/// </summary>
	public static PickupRevertPlan? SwapPlan(PickupRevertPlan? recorded, bool holdsRelic, bool teamHasSapphire,
		Func<int, bool> inDeck)
	{
		if (recorded is not { Reversible: true } || !holdsRelic || teamHasSapphire) return null;
		return recorded.Restore.Concat(recorded.Remove).All(inDeck) ? recorded : null;
	}
}
