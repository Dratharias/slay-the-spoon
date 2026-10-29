using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using SlaySpoon.Cards;

namespace SlaySpoon.CardSets;

/// <summary>
/// Hybrid card rewards: a duplicate gets a button that swaps it to its other version.
/// The swap is one of the reward's alternatives (the game's synced mechanism for Skip and Reroll): the chosen index
/// reaches every co-op client, and each one swaps its own copy of the reward, so decks stay identical.
/// </summary>
public static class RewardSwapPatch
{
	public const string OptionId = "SPOON_SWAP";

	/// <summary>
	/// Alternative k swaps card k. The count is fixed and ours come first, so an index means the same alternative in
	/// every list the game builds for one reward (the one OnSelect keeps and the screen's fresh one after a reroll).
	/// </summary>
	public const int SwapSlots = 8;

	private static readonly AccessTools.FieldRef<CardReward, List<CardCreationResult>> RewardCards =
		AccessTools.FieldRefAccess<CardReward, List<CardCreationResult>>("_cards");

	private static readonly AccessTools.FieldRef<CardReward, NCardRewardSelectionScreen?> ShownScreen =
		AccessTools.FieldRefAccess<CardReward, NCardRewardSelectionScreen?>("_currentlyShownScreen");

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, IReadOnlyList<CardCreationResult>> ScreenOptions =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, IReadOnlyList<CardCreationResult>>("_options");

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, IReadOnlyList<CardRewardAlternative>> ScreenAlternatives =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, IReadOnlyList<CardRewardAlternative>>("_extraOptions");

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, Control> AlternativesBar =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, Control>("_rewardAlternativesContainer");

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, TaskCompletionSource<int?>?> Completion =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, TaskCompletionSource<int?>?>("_completionSource");

	[HarmonyPatch(typeof(CardRewardAlternative), nameof(CardRewardAlternative.Generate))]
	public static class AddSwapAlternatives
	{
		public static void Postfix(CardReward cardReward, ref IReadOnlyList<CardRewardAlternative> __result)
		{
			if (!VersionSwap.IsHybrid(cardReward.Player.RunState))
			{
				return;
			}
			var alternatives = new List<CardRewardAlternative>(SwapSlots + __result.Count);
			for (int slot = 0; slot < SwapSlots; slot++)
			{
				int captured = slot;
				alternatives.Add(new CardRewardAlternative(OptionId, () => SwapSlot(cardReward, captured),
					PostAlternateCardRewardAction.DoNothing));
			}
			alternatives.AddRange(__result);
			__result = alternatives;
		}
	}

	/// <summary>Runs on every client when the owner of the reward picks the swap.</summary>
	private static Task SwapSlot(CardReward reward, int slot)
	{
		List<CardCreationResult> cards = RewardCards(reward);
		if (slot < cards.Count && VersionSwap.PartnerOf(cards[slot].Card) is { } partner)
		{
			cards[slot] = new CardCreationResult(VersionSwap.Swap(cards[slot].Card, partner));
			NCardRewardSelectionScreen? screen = ShownScreen(reward);
			screen?.RefreshOptions(cards, ScreenAlternatives(screen));
		}
		return Task.CompletedTask;
	}

	/// <summary>Replaces the game's buttons for our alternatives with one button per duplicate on screen.</summary>
	[HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.RefreshOptions))]
	public static class SwapButtons
	{
		public static void Postfix(NCardRewardSelectionScreen __instance, IReadOnlyList<CardCreationResult> options,
			IReadOnlyList<CardRewardAlternative> extraOptions)
		{
			if (extraOptions.Count < SwapSlots || extraOptions[0].OptionId != OptionId)
			{
				return;
			}
			Control bar = AlternativesBar(__instance);
			// The game just made one button per alternative, in order (older ones are already queued for deletion).
			foreach (NCardRewardAlternativeButton button in bar.GetChildren().OfType<NCardRewardAlternativeButton>()
				.Where(b => !b.IsQueuedForDeletion()).Take(SwapSlots).ToList())
			{
				button.QueueFreeSafely();
			}
			for (int slot = 0; slot < Math.Min(options.Count, SwapSlots); slot++)
			{
				CardModel card = options[slot].Card;
				if (VersionSwap.PartnerOf(card) == null)
				{
					continue;
				}
				int captured = slot;
				NCardRewardAlternativeButton button = NCardRewardAlternativeButton.Create(Label(card), MegaInput.viewDrawPile);
				bar.AddChildSafely(button);
				// TrySetResult: a second press before the screen reacts must not throw.
				button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ =>
					Completion(__instance)?.TrySetResult(ScreenOptions(__instance).Count + captured)));
			}
		}
	}

	private static string Label(CardModel card)
	{
		var label = new LocString("card_reward_ui", card is Sts1Card ? "SPOON_SWAP_TO_STS2.name" : "SPOON_SWAP_TO_STS1.name");
		label.Add("Card", card.TitleLocString);
		return label.GetFormattedText();
	}
}
