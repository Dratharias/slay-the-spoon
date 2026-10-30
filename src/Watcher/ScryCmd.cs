using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>Inspect the top cards, move selected cards to discard, then notify the holder's listeners.</summary>
public static class ScryCmd
{
	// Callers pass their action/hook context so a co-op selection yields through the existing action queue.
	public static async Task Scry(Player player, int amount, PlayerChoiceContext choiceContext)
	{
		if (player.PlayerCombatState is not { } combat || CombatManager.Instance.IsOverOrEnding) return;
		// Task 6 wires Golden Eye into this bonus when the relic exists.
		int count = ScryMath.CardsToShow(amount, combat.DrawPile.Cards.Count, goldenEye: false);
		var shown = combat.DrawPile.Cards.Take(count).ToList();
		int discarded = 0;
		if (shown.Count > 0)
		{
			var prefs = new CardSelectorPrefs(new LocString("card_selection", "SPOON_SCRY"), 0, shown.Count)
			{
				RequireManualConfirmation = true,
			};
			var chosen = (await CardSelectCmd.FromSimpleGrid(choiceContext, shown, player, prefs)).ToHashSet();
			if (CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState != combat) return;
			// Walk the revealed order, not click order. Unselected cards never move or shuffle.
			foreach (CardModel card in shown.Where(chosen.Contains))
			{
				if (!combat.DrawPile.Cards.Contains(card)) continue;
				// Scry moves from draw to discard; it is not a manual hand discard and must not trigger those effects.
				await CardPileCmd.Add(card, PileType.Discard);
				discarded++;
			}
		}
		// Snapshot after the moves: a Weave just discarded by this Scry must receive the notification too.
		var models = new List<AbstractModel>();
		models.AddRange(player.Creature.Powers);
		models.AddRange(player.Relics);
		models.AddRange(combat.Hand.Cards);
		models.AddRange(combat.DrawPile.Cards);
		models.AddRange(combat.DiscardPile.Cards);
		foreach (IScryListener listener in models.OfType<IScryListener>().Distinct().ToList())
			await listener.AfterScry(player, discarded);
	}
}
