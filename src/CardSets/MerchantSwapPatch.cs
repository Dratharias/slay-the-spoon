using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace SlaySpoon.CardSets;

/// <summary>
/// Hybrid merchant: a duplicate on sale gets a button above it that switches the version offered. Only the local
/// player sees and buys from their own offer, and a bought card reaches the other clients as a card, so the switch
/// stays local. The price stays the one rolled for the slot (no new roll, so seeds stay vanilla).
/// </summary>
public static class MerchantSwapPatch
{
	/// <summary>Above the card (its top edge is at -211), in the slot's coordinates (the slot is scaled 0.65).</summary>
	private static readonly Vector2 ButtonCenter = new(0f, -236f);

	private sealed record Versions(CardCreationResult First, CardCreationResult Second);

	private static readonly ConditionalWeakTable<NMerchantCard, NCardRewardAlternativeButton> Buttons = new();

	/// <summary>Both versions of an entry's card once switched, so switching back reuses the first card.</summary>
	private static readonly ConditionalWeakTable<MerchantCardEntry, Versions> Switched = new();

	private static readonly AccessTools.FieldRef<NMerchantCard, MerchantCardEntry> SlotEntry =
		AccessTools.FieldRefAccess<NMerchantCard, MerchantCardEntry>("_cardEntry");

	private static readonly Action<MerchantCardEntry, CardCreationResult?> SetCreationResult =
		AccessTools.MethodDelegate<Action<MerchantCardEntry, CardCreationResult?>>(
			AccessTools.PropertySetter(typeof(MerchantCardEntry), nameof(MerchantCardEntry.CreationResult)));

	/// <summary>Runs on fill and on every entry update (switch, purchase, restock, gold change).</summary>
	[HarmonyPatch(typeof(NMerchantCard), "UpdateVisual")]
	public static class Refresh
	{
		public static void Postfix(NMerchantCard __instance)
		{
			MerchantCardEntry entry = SlotEntry(__instance);
			CardCreationResult? offered = entry.CreationResult;
			bool switchable = offered != null && VersionSwap.PartnerOf(offered.Card) != null;
			if (!Buttons.TryGetValue(__instance, out NCardRewardAlternativeButton? button))
			{
				if (!switchable)
				{
					return;
				}
				button = SwapButton.Add(__instance, ButtonCenter, 1f, offered!.Card, () => SwitchVersion(entry));
				Buttons.Add(__instance, button);
			}
			SwapButton.SetVisible(button, switchable);
			if (switchable)
			{
				SwapButton.Relabel(button, offered!.Card);
			}
		}
	}

	private static void SwitchVersion(MerchantCardEntry entry)
	{
		CardCreationResult? offered = entry.CreationResult;
		if (offered == null || VersionSwap.PartnerOf(offered.Card) is not { } partner)
		{
			return;
		}
		CardCreationResult next;
		if (Switched.TryGetValue(entry, out Versions? versions) && (versions.First == offered || versions.Second == offered))
		{
			next = versions.First == offered ? versions.Second : versions.First;
		}
		else
		{
			// First switch, or the slot was restocked since: the stored pair belongs to an older card.
			next = new CardCreationResult(VersionSwap.Swap(offered.Card, partner));
			Switched.AddOrUpdate(entry, new Versions(offered, next));
		}
		SetCreationResult(entry, next);
		// Reapplies the merchant hooks (Fresnel Lens, eggs) to the new card, then redraws the slot.
		entry.OnMerchantInventoryUpdated();
	}
}
