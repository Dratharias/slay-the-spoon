using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Forced Trade (Custom Run, co-op, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): after each combat won,
/// every player picks one removable deck card on the synced removal screen, then each picked card moves to a random
/// other player (<see cref="ForcedTradeRule"/>), with its upgrade and enchantment (the Campfire Gifts move).
/// All picks happen before any card moves, so nobody gives away a card they just received.
/// </summary>
public sealed class SpoonForcedTrade : ModifierModel
{
	public override async Task AfterCombatVictory(CombatRoom room)
	{
		var run = RunState;
		var players = run.Players.ToList();
		if (!ForcedTradeRule.Applies(players.Count)) return;
		var picks = new List<(int slot, CardModel card)>();
		for (int slot = 0; slot < players.Count; slot++)
		{
			var player = players[slot];
			try
			{
				if (player.Creature.IsDead || !PileType.Deck.GetPile(player).Cards.Any(c => c.IsRemovable)) continue;
				var prompt = new LocString("card_selection", "SPOON_FORCED_TRADE");
				var picked = (await CardSelectCmd.FromDeckForRemoval(player, new CardSelectorPrefs(prompt, 1))).FirstOrDefault();
				if (picked != null) picks.Add((slot, picked));
			}
			catch (Exception e)
			{
				SpoonLog.Error($"forced trade: pick of slot {slot} failed: {e.Message}");
			}
		}
		foreach (var (slot, card) in picks)
		{
			try
			{
				var rng = new Rng(run.Rng.Seed, ForcedTradeRule.Stream(run.TotalFloor, slot));
				var receiver = players[ForcedTradeRule.Receiver(slot, players.Count, rng.NextInt(players.Count - 1))];
				if (card.Pile?.Type != PileType.Deck) continue;
				var copy = CardModel.FromSerializable(card.ToSerializable());
				await CardPileCmd.RemoveFromDeck(card);
				run.AddCard(copy, receiver);
				await CardPileCmd.Add(copy, PileType.Deck);
				SpoonLog.Info($"forced trade: {players[slot].NetId} gave {copy.Id.Entry} to {receiver.NetId}");
			}
			catch (Exception e)
			{
				SpoonLog.Error($"forced trade: move from slot {slot} failed: {e.Message}");
			}
		}
	}
}
