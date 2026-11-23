namespace SlaySpoon.Core;

/// <summary>Rules of the Overcursed modifier (spec docs/superpowers/specs/2026-11-23-overcursed-design.md).</summary>
public static class OvercursedRule
{
	/// <summary>Copies added with every curse that enters a deck: three of each in all.</summary>
	public const int ExtraCopies = 2;

	/// <summary>
	/// Copies Overcursed adds for a card entering a deck: two for a curse, none for other cards, none when vanilla Hoarder or
	/// Matriochka already copy every card (no five of a kind), none at Matriochka's deck limit.
	/// </summary>
	public static int CopiesFor(bool curse, bool hoarderOrMatriochka, int deckCount) =>
		!curse || hoarderOrMatriochka ? 0 : Math.Clamp(MatriochkaRule.DeckLimit - deckCount, 0, ExtraCopies);

	/// <summary>Whether events get "Embrace the curse": Cursed Deck or Overcursed.</summary>
	public static bool EmbraceOffered(bool cursedDeck, bool overcursed) => cursedDeck || overcursed;

	/// <summary>
	/// The Embrace reward with the run's modes: Blood turns gold into a heal (gold is HP); King Midas always pays gold, doubled.
	/// </summary>
	public static CurseEventReward EmbraceReward(CurseEventReward drawn, bool blood, bool midas) =>
		blood && drawn == CurseEventReward.Gold ? CurseEventReward.Heal
		: midas && !blood ? CurseEventReward.Gold
		: drawn;

	public static int EmbraceGold(bool midas) => midas ? 2 * CurseRule.EventGold : CurseRule.EventGold;
}
