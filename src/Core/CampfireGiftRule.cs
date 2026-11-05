namespace SlaySpoon.Core;

/// <summary>
/// Campfire Gifts (Custom Run, co-op): a player spends their rest-site action to give a deck card to an ally, who pays
/// <see cref="Price"/> gold for it. A trade is two gifts at the same fire, so each player pays the price once.
/// </summary>
public static class CampfireGiftRule
{
	public const int Price = 50;

	/// <summary>An ally can receive while alive, able to pay, and not the giver.</summary>
	public static bool CanReceive(bool isGiver, bool alive, int gold) => !isGiver && alive && gold >= Price;

	/// <summary>Curses cannot be gifted: the receiver would pay to take one (builder call). Eternal cards stay too.</summary>
	public static bool CanGift(string cardType, bool removable) => removable && cardType != "Curse";
}
