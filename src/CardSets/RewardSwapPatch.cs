using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;

namespace SlaySpoon.CardSets;

/// <summary>
/// Hybrid card rewards: a duplicate gets a button under it that swaps it to its other version.
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

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, Control> CardRow =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, Control>("_cardRow");

	private static readonly AccessTools.FieldRef<NCardRewardSelectionScreen, TaskCompletionSource<int?>?> Completion =
		AccessTools.FieldRefAccess<NCardRewardSelectionScreen, TaskCompletionSource<int?>?>("_completionSource");

	[HarmonyPatch(typeof(CardRewardAlternative), nameof(CardRewardAlternative.Generate))]
	public static class AddSwapAlternatives
	{
		public static void Postfix(CardReward cardReward, ref IReadOnlyList<CardRewardAlternative> __result)
		{
			if (!VersionSwap.IsHybrid(cardReward.Player))
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

	/// <summary>
	/// Replaces the game's buttons for our alternatives with one button under each duplicate (a name in the button
	/// bar next to Skip is too long for its frame). A press picks that card's alternative, like the game's own buttons.
	/// </summary>
	[HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.RefreshOptions))]
	public static class SwapButtons
	{
		/// <summary>The screen's card spacing; its card row's origin is the middle card's center.</summary>
		private const float CardSpacing = 350f;

		private const string HolderMeta = "spoon_reward_swap";

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
			Control row = CardRow(__instance);
			// The game only clears its own card holders from the row; a swap refreshes the screen with new buttons.
			foreach (Control old in row.GetChildren().OfType<Control>().Where(c => c.HasMeta(HolderMeta)).ToList())
			{
				old.QueueFreeSafely();
			}
			for (int slot = 0; slot < Math.Min(options.Count, SwapSlots); slot++)
			{
				CardModel card = options[slot].Card;
				if (VersionSwap.PartnerOf(card) == null)
				{
					continue;
				}
				int captured = slot;
				var center = new Vector2((slot - (options.Count - 1) / 2f) * CardSpacing, SwapButton.BelowCardY);
				// TrySetResult: a second press before the screen reacts must not throw.
				NCardRewardAlternativeButton button = SwapButton.Add(row, center, SwapButton.BelowCardScale, card,
					() => Completion(__instance)?.TrySetResult(ScreenOptions(__instance).Count + captured));
				button.GetParent().SetMeta(HolderMeta, true);
			}
		}
	}
}
