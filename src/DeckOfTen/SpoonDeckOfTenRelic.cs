using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.DeckOfTen;

/// <summary>
/// Deck of 10 (<see cref="DeckOfTenRule"/>): the holder's deck holds at most 10 cards, each dropped card gives +1 max HP.
/// The first trim is the Sealed Deck grid (pick the cards to keep); later trims pick the cards to drop on the removal
/// screen. Given at the start by the Custom Run mode, or offered by Neow in a Standard run (<see cref="DeckOfTenNeowPatch"/>).
/// </summary>
public sealed class SpoonDeckOfTenRelic : RelicModel
{
	private static readonly Func<CardModel, CardModel, int> CompareSealed =
		AccessTools.MethodDelegate<Func<CardModel, CardModel, int>>(AccessTools.Method(typeof(SealedDeck), "CompareCards"));

	private bool _sealed;
	private bool _trimming;

	public override RelicRarity Rarity => RelicRarity.Event;

	/// <summary>The mod ships no relic art: the icons come from Precarious Shears.</summary>
	protected override string IconBaseName => "precarious_shears";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Limit", DeckOfTenRule.Limit),
		new DynamicVar("MaxHp", DeckOfTenRule.MaxHpPerDrop),
	};

	/// <summary>Whether the keep grid already ran: later trims use the removal screen.</summary>
	[SavedProperty]
	public bool SpoonDeckOfTenSealed
	{
		get => _sealed;
		set { AssertMutable(); _sealed = value; }
	}

	public override Task AfterObtained() => Trim();

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (card.Owner != Owner || card.Pile?.Type != PileType.Deck || oldPileType == PileType.Deck) return;
		if (CombatManager.Instance.IsInProgress) return;
		await Trim();
	}

	public override Task AfterCombatEnd(CombatRoom room) => Trim();

	/// <summary>
	/// The start-of-run trim (a starter deck over 10, or a curse from a starting relic). Not AfterRoomEntered: rooms
	/// are entered while the screen is faded to black, so a selection screen opened there stays hidden and the fade
	/// never lifts (black screen). AfterActEntered runs after the fade-in.
	/// </summary>
	public override Task AfterActEntered() => Trim();

	private async Task Trim()
	{
		if (Owner == null || Owner.Creature.IsDead || _trimming) return;
		// FinalizeStartingRelics runs before RunManager.Launch sets the local player: a selection then waits for a
		// "remote" choice and throws. AfterActEntered trims once the run is on screen.
		if (LocalContext.NetId == null) return;
		_trimming = true;
		try
		{
			var deck = PileType.Deck.GetPile(Owner).Cards;
			var removable = deck.Where(c => c.IsRemovable).ToList();
			int drop = DeckOfTenRule.ToDrop(deck.Count, removable.Count);
			if (drop == 0) return;
			List<CardModel> dropped;
			if (!SpoonDeckOfTenSealed)
			{
				int keep = DeckOfTenRule.ToKeep(deck.Count, removable.Count);
				var prompt = new LocString("card_selection", "SPOON_DECK_OF_TEN_KEEP");
				prompt.Add("Count", keep);
				var prefs = new CardSelectorPrefs(prompt, keep)
				{
					Cancelable = false,
					RequireManualConfirmation = true,
					Comparison = (a, b) => CompareSealed(a, b),
				};
				var kept = keep == 0
					? new HashSet<CardModel>()
					: (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), removable, Owner, prefs)).ToHashSet();
				dropped = removable.Where(c => !kept.Contains(c)).Take(drop).ToList();
				SpoonDeckOfTenSealed = true;
			}
			else
			{
				var prompt = new LocString("card_selection", "SPOON_DECK_OF_TEN");
				prompt.Add("Count", drop);
				dropped = (await CardSelectCmd.FromDeckForRemoval(Owner, new CardSelectorPrefs(prompt, drop))).ToList();
			}
			if (dropped.Count == 0) return;
			Flash();
			await CardPileCmd.RemoveFromDeck(dropped);
			await CreatureCmd.GainMaxHp(Owner.Creature, dropped.Count * DeckOfTenRule.MaxHpPerDrop);
			SpoonLog.Info($"deck of 10: {Owner.NetId} dropped {string.Join(", ", dropped.Select(c => c.Id.Entry))}");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"deck of 10: trim failed: {e.Message}");
		}
		finally
		{
			_trimming = false;
		}
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class DeckOfTenRelicPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonDeckOfTenRelic>()).ToList();
	}
}
