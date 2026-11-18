using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Deck of 10 (Custom Run, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): a deck above 10 cards makes its
/// owner drop the excess on the vanilla removal screen; each dropped card gives +1 max HP (<see cref="DeckOfTenRule"/>).
/// Checked when a card joins the deck out of combat, when combat ends and when a room is entered (starting decks).
/// </summary>
public sealed class SpoonDeckOfTen : ModifierModel
{
	private readonly HashSet<ulong> _trimming = new();

	public static bool IsActive(MegaCrit.Sts2.Core.Runs.IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonDeckOfTen);

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (card.Pile?.Type != PileType.Deck || oldPileType == PileType.Deck) return;
		if (CombatManager.Instance.IsInProgress) return;
		await Trim(card.Owner);
	}

	public override async Task AfterCombatEnd(CombatRoom room)
	{
		foreach (var player in RunState.Players.ToList()) await Trim(player);
	}

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom) return;
		foreach (var player in RunState.Players.ToList()) await Trim(player);
	}

	/// <summary>Drops the excess over 10: the owner picks, each dropped card gives +1 max HP.</summary>
	public async Task Trim(Player? player)
	{
		if (player == null || player.Creature.IsDead || !_trimming.Add(player.NetId)) return;
		try
		{
			var deck = PileType.Deck.GetPile(player).Cards;
			int drop = DeckOfTenRule.ToDrop(deck.Count, deck.Count(c => c.IsRemovable));
			if (drop == 0) return;
			var prompt = new LocString("card_selection", "SPOON_DECK_OF_TEN");
			prompt.Add("Count", drop);
			var picked = (await CardSelectCmd.FromDeckForRemoval(player, new CardSelectorPrefs(prompt, drop))).ToList();
			if (picked.Count == 0) return;
			await CardPileCmd.RemoveFromDeck(picked);
			await CreatureCmd.GainMaxHp(player.Creature, picked.Count * DeckOfTenRule.MaxHpPerDrop);
			SpoonLog.Info($"deck of 10: {player.NetId} dropped {string.Join(", ", picked.Select(c => c.Id.Entry))}");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"deck of 10: trim failed: {e.Message}");
		}
		finally
		{
			_trimming.Remove(player.NetId);
		}
	}
}
