using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace SlaySpoon.CardSets;

/// <summary>
/// Hybrid reward grids (Sealed Deck, Sea Glass, Brain Leech, Room Full of Cheese): the grid has no room for a switch
/// under each card, so once the player confirms, each duplicate they kept gets a "choose a version" screen with
/// both versions side by side. Each answer is a synced choice; every client makes the version picked.
/// Grids the game skips (too few cards to choose from) and test selectors run vanilla.
/// </summary>
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGridForRewards))]
public static class GridVersionPatch
{
	private static readonly AccessTools.FieldRef<NChooseACardSelectionScreen, NCommonBanner> Banner =
		AccessTools.FieldRefAccess<NChooseACardSelectionScreen, NCommonBanner>("_banner");

	public static void Postfix(PlayerChoiceContext context, List<CardCreationResult> cards, Player player,
		CardSelectorPrefs prefs, ref Task<IEnumerable<CardModel>> __result)
	{
		bool screenShown = prefs.RequireManualConfirmation || cards.Count > prefs.MinSelect;
		if (CardSelectCmd.Selector != null || !screenShown || !VersionSwap.IsHybrid(player))
		{
			return;
		}
		__result = WithVersionSteps(__result, context, player);
	}

	private static async Task<IEnumerable<CardModel>> WithVersionSteps(Task<IEnumerable<CardModel>> grid,
		PlayerChoiceContext context, Player player)
	{
		List<CardModel> kept = (await grid).ToList();
		for (int i = 0; i < kept.Count; i++)
		{
			CardModel card = kept[i];
			if (VersionSwap.PartnerOf(card) is not { } partner)
			{
				continue;
			}
			int pick = await SyncedChoice.Choose(context, player, () => PickVersion(card, partner));
			if (pick == 1)
			{
				kept[i] = VersionSwap.Swap(card, partner);
			}
		}
		return kept;
	}

	/// <summary>0 keeps the card, 1 takes the other version (shown here as a local copy).</summary>
	private static async Task<int> PickVersion(CardModel card, CardModel partner)
	{
		CardModel other = VersionSwap.Swap(card, partner);
		NChooseACardSelectionScreen screen = NChooseACardSelectionScreen.ShowScreen(new[] { card, other }, canSkip: false)!;
		SwapButton.WhenReady(screen, () => Banner(screen).label.SetTextAutoSize(
			new LocString("card_reward_ui", "SPOON_CHOOSE_VERSION_HEADER").GetFormattedText()));
		CardModel? picked = (await screen.CardsSelected()).FirstOrDefault();
		return picked == other ? 1 : 0;
	}
}
