using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace SlaySpoon.Watcher;

/// <summary>Previous completed play for this player; the current OnPlay has not finished yet.</summary>
public static class LastCardPlayed
{
	public static CardType? Type(Player player)
	{
		if (player.PlayerCombatState == null || !CombatManager.Instance.IsInProgress) return null;
		return CombatManager.Instance.History.CardPlaysFinished
			.LastOrDefault(entry => entry.CardPlay.Card.Owner == player)?.CardPlay.Card.Type;
	}
}
