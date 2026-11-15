using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Hybrid choose-a-card screens (Discovery, potions, Toolbox, Massive Scroll...): each duplicate gets a button under
/// it that switches the version shown. The pick travels as one synced index that also carries the version
/// (<see cref="VersionChoiceCode"/>), and every client makes the other version from it, so decks stay identical.
/// Screens without a duplicate, and test selectors, run vanilla.
/// </summary>
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
public static class ChooseACardSwapPatch
{
	/// <summary>The screen's card spacing; its card row's origin is the middle card's center.</summary>
	private const float CardSpacing = 340f;

	private static readonly AccessTools.FieldRef<NChooseACardSelectionScreen, Control> CardRow =
		AccessTools.FieldRefAccess<NChooseACardSelectionScreen, Control>("_cardRow");

	public static bool Prefix(PlayerChoiceContext context, IReadOnlyList<CardModel> cards, Player player, bool canSkip,
		ref Task<CardModel?> __result)
	{
		if (CardSelectCmd.Selector != null || Midas.MidasCardPicks.Blocks(player) || cards.Count is < 1 or > 3
			|| !cards.Any(c => VersionSwap.PartnerOf(c) != null))
		{
			return true;
		}
		__result = Choose(context, cards, player, canSkip);
		return false;
	}

	private static async Task<CardModel?> Choose(PlayerChoiceContext context, IReadOnlyList<CardModel> cards,
		Player player, bool canSkip)
	{
		int code = await SyncedChoice.Choose(context, player, () => PickLocally(cards, canSkip));
		(int index, bool swapped) = VersionChoiceCode.Decode(code, cards.Count);
		CardModel? result = index < 0 ? null : cards[index];
		if (result != null && swapped && VersionSwap.PartnerOf(result) is { } partner)
		{
			result = VersionSwap.Swap(result, partner);
		}
		Log.Info($"Player {player.NetId} chose cards [{result?.Id.Entry}]");
		return result;
	}

	/// <summary>
	/// The vanilla screen plus the buttons. Each switch shows a local copy of the other version (made once); the
	/// copy is only for display, the synced code says which version every client creates.
	/// </summary>
	private static async Task<int> PickLocally(IReadOnlyList<CardModel> cards, bool canSkip)
	{
		NPlayerHand.Instance?.CancelAllCardPlay();
		NChooseACardSelectionScreen screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip)!;
		foreach (CardModel card in cards)
		{
			SaveManager.Instance.MarkCardAsSeen(card);
		}
		var others = new CardModel?[cards.Count];
		SwapButton.WhenReady(screen, () => AddButtons(screen, cards, others));

		CardModel? picked = (await screen.CardsSelected()).FirstOrDefault();
		for (int i = 0; i < cards.Count; i++)
		{
			if (picked == cards[i] || (picked != null && picked == others[i]))
			{
				return VersionChoiceCode.Encode(i, picked != cards[i], cards.Count);
			}
		}
		return -1;
	}

	private static void AddButtons(NChooseACardSelectionScreen screen, IReadOnlyList<CardModel> cards,
		CardModel?[] others)
	{
		Control row = CardRow(screen);
		List<NGridCardHolder> holders = row.GetChildren().OfType<NGridCardHolder>().ToList();
		for (int i = 0; i < cards.Count && i < holders.Count; i++)
		{
			if (VersionSwap.PartnerOf(cards[i]) is not { } partner)
			{
				continue;
			}
			int slot = i;
			NGridCardHolder holder = holders[i];
			var center = new Vector2((i - (cards.Count - 1) / 2f) * CardSpacing, SwapButton.BelowCardY);
			NCardRewardAlternativeButton? button = null;
			button = SwapButton.Add(row, center, SwapButton.BelowCardScale, cards[i], () =>
			{
				if (holder.CardNode is not { } node)
				{
					return;
				}
				others[slot] ??= VersionSwap.Swap(cards[slot], partner);
				CardModel next = node.Model == cards[slot] ? others[slot]! : cards[slot];
				node.Model = next;
				node.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
				SwapButton.Relabel(button!, next);
			});
		}
	}
}
