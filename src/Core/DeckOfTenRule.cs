namespace SlaySpoon.Core;

/// <summary>
/// Deck of 10 (the relic, and the Custom Run mode that hands it out): the deck holds at most 10 cards; each dropped card
/// gives +1 max HP. The first trim picks the cards to keep on the Sealed Deck grid; later trims pick the cards to drop.
/// </summary>
public static class DeckOfTenRule
{
	public const int Limit = 10;
	public const int MaxHpPerDrop = 1;

	/// <summary>Neow's cursed slot holds the relic 1 time in 11: an equal share with the 10 vanilla cursed relics.</summary>
	public const int NeowOdds = 11;

	/// <summary>How many cards to drop: the excess, bounded by the cards the player is allowed to pick.</summary>
	public static int ToDrop(int deckSize, int removable) => Math.Clamp(deckSize - Limit, 0, Math.Max(removable, 0));

	/// <summary>How many removable cards the keep grid asks for: unremovable cards take their place among the 10.</summary>
	public static int ToKeep(int deckSize, int removable) => Math.Max(removable, 0) - ToDrop(deckSize, removable);

	public static bool OfferedAtNeow(int roll) => roll == 0;

	public static string NeowStream(int slot) => $"spoon_deck_of_ten_neow_{slot}";
}
