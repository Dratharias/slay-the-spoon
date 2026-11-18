namespace SlaySpoon.Core;

/// <summary>Deck of 10 (Custom Run): the deck holds at most 10 cards; each dropped card gives +1 max HP.</summary>
public static class DeckOfTenRule
{
	public const int Limit = 10;
	public const int MaxHpPerDrop = 1;

	/// <summary>How many cards to drop: the excess, bounded by the cards the player is allowed to pick.</summary>
	public static int ToDrop(int deckSize, int removable) => Math.Clamp(deckSize - Limit, 0, Math.Max(removable, 0));
}
