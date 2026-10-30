using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// Sapphire chest swap. The pickup records live in a run-scoped ledger filled by game logic every peer runs (the chest's
/// award and <see cref="RelicCmd.Obtain"/>), so the queued swap decides from run state only. The room UI only decides
/// when the button shows. Nothing is serialized: a resumed chest has no stale swap offer.
/// </summary>
internal static class SapphireChestPatch
{
	private static readonly JsonSerializerOptions CardJson = new() { IncludeFields = true };

	/// <summary>What one chest relic's pickup changed, recorded on every peer.</summary>
	internal sealed class Entry(Player player)
	{
		public readonly Player Player = player;
		public readonly Dictionary<CardModel, int> Handles = new();
		public readonly Dictionary<int, SerializableCard> Saved = new();
		public PickupSnapshot? Before;
		public PickupRevertPlan? Plan;
		public PotionModel[] AddedPotions = [];
		public PotionModel[] BeforePotions = [];

		public PickupSnapshot Snapshot(bool before)
		{
			var deck = new Dictionary<int, string>();
			foreach (var card in Player.Deck.Cards)
			{
				if (!Handles.TryGetValue(card, out int handle)) Handles[card] = handle = Handles.Count;
				var save = card.ToSerializable();
				deck[handle] = JsonSerializer.Serialize(save, CardJson);
				if (before) Saved[handle] = save;
			}
			return new(deck, Player.Gold, Player.Creature.MaxHp, Player.Potions.Count(), Player.Creature.CurrentHp,
				Player.MaxPotionCount);
		}
	}

	/// <summary>Local UI view of a chest award: only used to know when every pickup's effects have finished.</summary>
	internal sealed class Pickup(Player player, ModelId id)
	{
		public readonly Player Player = player;
		public readonly ModelId Id = id;
		public readonly TaskCompletionSource Ready = new();
		public RelicModel? Relic;
	}

	private sealed record ObtainState(Entry? Entry, Pickup? Pickup);

	// Game-logic state, the same on every peer.
	private static readonly PickupLedger<(Player, RelicModel), Entry> Ledger = new();
	private static IRunState? _ledgerRun;

	// Local UI state.
	private static readonly List<Pickup> Pickups = [];
	private static IRunState? _run;
	private static object? _room;
	private static event Action? Changed;
	private static readonly AccessTools.FieldRef<NTreasureRoom, IRunState> Run =
		AccessTools.FieldRefAccess<NTreasureRoom, IRunState>("_runState");

	private static void ClearLedger()
	{
		Ledger.Clear();
		_ledgerRun = null;
	}

	private static void ClearUi()
	{
		_run = null;
		_room = null;
		foreach (var pickup in Pickups) pickup.Ready.TrySetResult();
		Pickups.Clear();
	}

	/// <summary>A new or loaded run starts with an empty ledger.</summary>
	[HarmonyPatch(typeof(RunManager), "InitializeShared")]
	private static class RunStartPatch
	{
		private static void Postfix() => ClearLedger();
	}

	/// <summary>Leaving a room clears the ledger in the synchronized room flow, before the next room starts.</summary>
	[HarmonyPatch(typeof(RunManager), "ExitCurrentRooms")]
	private static class RoomExitPatch
	{
		private static void Prefix() => ClearLedger();
	}

	/// <summary>Every peer runs the award once all votes are in: remember which relic ids this chest hands out.</summary>
	[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), "AwardRelics")]
	private static class AwardPatch
	{
		private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, List<RelicModel>?> Current =
			AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, List<RelicModel>?>("_currentRelics");
		private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, IPlayerCollection> Players =
			AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, IPlayerCollection>("_playerCollection");

		private static void Prefix(TreasureRoomRelicSynchronizer __instance)
		{
			if (Current(__instance) is not { Count: > 0 } relics || Players(__instance) is not IRunState run) return;
			if (SpoonAct4.Of(run) is not { } act4 || act4.Keys.Has(Act4Key.Sapphire)) return;
			if (_ledgerRun != run) ClearLedger();
			_ledgerRun = run;
			Ledger.Expect(relics.Select(r => r.Id.ToString()));
		}
	}

	[HarmonyPatch(typeof(NTreasureRoom), nameof(NTreasureRoom._Ready))]
	private static class RoomPatch
	{
		private static void Postfix(NTreasureRoom __instance)
		{
			ClearUi();
			ClearLedger();
			var run = Run(__instance);
			if (SpoonAct4.Of(run) is not { } act4 || act4.Keys.Has(Act4Key.Sapphire)) return;
			_run = run;
			_room = run.CurrentRoom;
			var me = LocalContext.GetMe(run);
			if (me == null) return;
			var button = new Button { Text = new LocString("merchant_room", "SPOON_SAPPHIRE_SWAP").GetFormattedText(),
				Position = new Vector2(650, 770), Visible = false };
			button.AddThemeFontSizeOverride("font_size", 24);
			__instance.AddChild(button);
			RelicModel? offered = null;
			void Refresh()
			{
				// Wait in the UI, not in the queued action, until every pickup's own effects have finished.
				offered = Pickups.FirstOrDefault(p => p.Player == me && p.Ready.Task.IsCompleted && p.Relic != null
					&& Ledger.TryFind((me, p.Relic), run.TotalFloor, out var entry) && entry.Plan?.Reversible == true)?.Relic;
				button.Visible = offered != null && !act4.Keys.Has(Act4Key.Sapphire) && Pickups.All(p => p.Ready.Task.IsCompleted);
				if (offered is { } relic)
				{
					var tip = new LocString("merchant_room", "SPOON_SAPPHIRE_SWAP_TIP");
					tip.Add("Relic", relic.Title.GetFormattedText());
					button.TooltipText = tip.GetFormattedText();
				}
			}
			button.Pressed += () =>
			{
				if (offered is not { } relic || button.Disabled) return;
				int index = me.Relics.IndexOf(relic);
				if (index < 0) return;
				button.Disabled = true;
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new SapphireSwapAction(me,
					new NetSapphireSwap { RelicIndex = index, Floor = run.TotalFloor }));
			};
			Changed += Refresh;
			Act4Keys.Changed += Refresh;
			__instance.TreeExiting += () =>
			{
				Changed -= Refresh;
				Act4Keys.Changed -= Refresh;
				ClearUi();
			};
		}
	}

	[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "AnimateRelicAwards")]
	private static class AwardsPatch
	{
		private static void Prefix(List<RelicPickingResult> results)
		{
			if (_run == null || _run.CurrentRoom != _room) return;
			foreach (var result in results.Where(r => r.player != null && r.type != RelicPickingResultType.Skipped))
				Pickups.Add(new Pickup(result.player!, result.relic.Id));
		}
	}

	[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), [typeof(RelicModel), typeof(Player), typeof(int)])]
	private static class ObtainPatch
	{
		private static void Prefix(RelicModel relic, Player player, out ObtainState? __state)
		{
			__state = null;
			var run = player.RunState;
			Entry? entry = null;
			if (run == _ledgerRun && Ledger.ConsumeExpected(relic.Id.ToString()))
			{
				entry = new Entry(player);
				entry.Before = entry.Snapshot(before: true);
				entry.BeforePotions = player.Potions.ToArray();
				Ledger.Record((player, relic), run.TotalFloor, entry);
			}
			var pickup = _run == run && run.CurrentRoom == _room
				? Pickups.FirstOrDefault(p => p.Player == player && p.Id == relic.Id && p.Relic == null) : null;
			if (pickup != null) pickup.Relic = relic;
			if (entry != null || pickup != null) __state = new(entry, pickup);
		}
		private static void Postfix(ObtainState? __state, ref Task<RelicModel> __result)
		{
			if (__state != null) __result = Finish(__result, __state);
		}
		private static async Task<RelicModel> Finish(Task<RelicModel> task, ObtainState state)
		{
			try
			{
				var relic = await task;
				if (state.Entry is { } entry)
				{
					entry.Plan = SapphireRevert.Plan(entry.Before!, entry.Snapshot(before: false));
					entry.AddedPotions = entry.Player.Potions.Except(entry.BeforePotions).ToArray();
				}
				return relic;
			}
			finally
			{
				state.Pickup?.Ready.TrySetResult();
				Changed?.Invoke();
			}
		}
	}

	/// <summary>
	/// The queued swap. It reads run state only and never waits: a failed check returns before the key is taken, the
	/// same way on every peer.
	/// </summary>
	internal static async Task Swap(Player player, NetSapphireSwap action)
	{
		var run = player.RunState;
		if (run != _ledgerRun || action.RelicIndex < 0 || action.RelicIndex >= player.Relics.Count) return;
		var relic = player.Relics[action.RelicIndex];
		var act4 = SpoonAct4.Of(run);
		Ledger.TryFind((player, relic), action.Floor, out var entry);
		var cards = entry?.Handles.ToDictionary(p => p.Value, p => p.Key);
		var plan = SapphireRevert.SwapPlan(entry?.Plan, player.Relics.Contains(relic),
			act4 == null || act4.Keys.Has(Act4Key.Sapphire),
			h => cards != null && cards.TryGetValue(h, out var card) && player.Deck.Cards.Contains(card));
		if (plan == null || entry == null || cards == null || act4 == null) return;
		if (!act4.Keys.Take(Act4Key.Sapphire)) return;
		foreach (int handle in plan.Restore)
		{
			var original = cards[handle];
			int index = player.Deck.Cards.IndexOf(original);
			var restored = CardModel.FromSerializable(entry.Saved[handle]);
			run.AddCard(restored, player);
			original.RemoveFromState();
			player.Deck.AddInternal(restored, index);
		}
		foreach (int handle in plan.Remove) cards[handle].RemoveFromState();
		if (plan.GoldToLose > 0) await PlayerCmd.LoseGold(plan.GoldToLose, player);
		var creature = player.Creature;
		int hpBefore = creature.CurrentHp;
		if (plan.MaxHpToLose > 0)
			await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, plan.MaxHpToLose, isFromCard: false);
		// GainMaxHp also healed: take back what the max HP clamp did not already take, by setting HP (no damage hooks).
		int hpLoss = SapphireRevert.HpLoss(plan.HpToLose, hpBefore - creature.CurrentHp, creature.CurrentHp);
		if (hpLoss > 0) await CreatureCmd.SetCurrentHp(creature, creature.CurrentHp - hpLoss);
		foreach (var potion in entry.AddedPotions.Where(p => player.Potions.Contains(p)).Take(plan.PotionsToDiscard))
			await PotionCmd.Discard(potion);
		// After the discards, so the potions the player kept move into the remaining slots.
		if (plan.PotionSlotsToLose > 0) await PlayerCmd.LoseMaxPotionCount(plan.PotionSlotsToLose, player);
		await RelicCmd.Remove(relic);
		SpoonLog.Info($"act4: Sapphire swap {player.NetId} {relic.Id}: restore {plan.Restore.Count}, remove {plan.Remove.Count}, "
			+ $"gold {plan.GoldToLose}, max HP {plan.MaxHpToLose}, HP {hpLoss}, potion slots {plan.PotionSlotsToLose}");
	}
}
