using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>Room-scoped chest pickup records. Nothing is serialized: a resumed chest has no stale swap offer.</summary>
internal static class SapphireChestPatch
{
	private static readonly JsonSerializerOptions CardJson = new() { IncludeFields = true };
	internal sealed class Pickup(Player player, ModelId id)
	{
		public readonly Player Player = player;
		public readonly ModelId Id = id;
		public readonly TaskCompletionSource Ready = new();
		public readonly Dictionary<CardModel, int> Handles = new();
		public readonly Dictionary<int, SerializableCard> Saved = new();
		public RelicModel? Relic;
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
			return new(deck, Player.Gold, Player.Creature.MaxHp, Player.Potions.Count());
		}
	}

	private static readonly List<Pickup> Pickups = [];
	private static IRunState? _run;
	private static object? _room;
	private static TaskCompletionSource _awardsStarted = new();
	private static event Action? Changed;
	private static readonly AccessTools.FieldRef<NTreasureRoom, IRunState> Run =
		AccessTools.FieldRefAccess<NTreasureRoom, IRunState>("_runState");

	private static void Clear()
	{
		// Invalidate first: completing a task can resume a waiting swap synchronously.
		_run = null;
		_room = null;
		_awardsStarted.TrySetResult();
		_awardsStarted = new();
		foreach (var pickup in Pickups) pickup.Ready.TrySetResult();
		Pickups.Clear();
	}

	[HarmonyPatch(typeof(NTreasureRoom), nameof(NTreasureRoom._Ready))]
	private static class RoomPatch
	{
		private static void Postfix(NTreasureRoom __instance)
		{
			Clear();
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
			Pickup? offered = null;
			void Refresh()
			{
				offered = Pickups.FirstOrDefault(p => p.Player == me && p.Ready.Task.IsCompleted && p.Plan?.Reversible == true && p.Relic != null);
				button.Visible = offered != null && !act4.Keys.Has(Act4Key.Sapphire) && Pickups.All(p => p.Ready.Task.IsCompleted);
				if (offered?.Relic is { } relic)
				{
					var tip = new LocString("merchant_room", "SPOON_SAPPHIRE_SWAP_TIP");
					tip.Add("Relic", relic.Title.GetFormattedText());
					button.TooltipText = tip.GetFormattedText();
				}
			}
			button.Pressed += () =>
			{
				if (offered?.Relic is not { } relic || button.Disabled) return;
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
				Clear();
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
			_awardsStarted.TrySetResult();
		}
	}

	[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), [typeof(RelicModel), typeof(Player), typeof(int)])]
	private static class ObtainPatch
	{
		private static void Prefix(RelicModel relic, Player player, out Pickup? __state)
		{
			__state = _run == player.RunState && _run.CurrentRoom == _room
				? Pickups.FirstOrDefault(p => p.Player == player && p.Id == relic.Id && p.Relic == null) : null;
			if (__state == null) return;
			__state.Relic = relic;
			__state.Before = __state.Snapshot(before: true);
			__state.BeforePotions = player.Potions.ToArray();
		}
		private static void Postfix(Pickup? __state, ref Task<RelicModel> __result)
		{
			if (__state != null) __result = Finish(__result, __state);
		}
		private static async Task<RelicModel> Finish(Task<RelicModel> task, Pickup pickup)
		{
			try
			{
				var relic = await task;
				pickup.Plan = SapphireRevert.Plan(pickup.Before!, pickup.Snapshot(before: false));
				pickup.AddedPotions = pickup.Player.Potions.Except(pickup.BeforePotions).ToArray();
				return relic;
			}
			finally { pickup.Ready.TrySetResult(); Changed?.Invoke(); }
		}
	}

	internal static async Task Swap(Player player, NetSapphireSwap action)
	{
		var run = player.RunState;
		if (_run != run || run.CurrentRoom != _room || run.TotalFloor != action.Floor || action.RelicIndex < 0) return;
		// Other peers may still be animating the chest when this queued choice reaches them.
		await _awardsStarted.Task;
		if (_run != run || run.CurrentRoom != _room || run.TotalFloor != action.Floor) return;
		await Task.WhenAll(Pickups.Select(p => p.Ready.Task));
		if (_run != run || run.CurrentRoom != _room || action.RelicIndex >= player.Relics.Count) return;
		var relic = player.Relics[action.RelicIndex];
		var pickup = Pickups.FirstOrDefault(p => p.Player == player && p.Relic == relic);
		if (pickup?.Plan is not { Reversible: true } plan || SpoonAct4.Of(run) is not { } act4) return;
		var cards = pickup.Handles.ToDictionary(p => p.Value, p => p.Key);
		if (plan.Restore.Concat(plan.Remove).Any(h => !player.Deck.Cards.Contains(cards[h]))) return;
		if (!act4.Keys.Take(Act4Key.Sapphire)) return;
		foreach (int handle in plan.Restore)
		{
			var original = cards[handle];
			int index = player.Deck.Cards.IndexOf(original);
			var restored = CardModel.FromSerializable(pickup.Saved[handle]);
			run.AddCard(restored, player);
			original.RemoveFromState();
			player.Deck.AddInternal(restored, index);
		}
		foreach (int handle in plan.Remove) cards[handle].RemoveFromState();
		if (plan.GoldToLose > 0) await PlayerCmd.LoseGold(plan.GoldToLose, player);
		if (plan.MaxHpToLose > 0)
			await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), player.Creature, plan.MaxHpToLose, isFromCard: false);
		foreach (var potion in pickup.AddedPotions.Where(p => player.Potions.Contains(p)).Take(plan.PotionsToDiscard))
			await PotionCmd.Discard(potion);
		await RelicCmd.Remove(relic);
		SpoonLog.Info($"act4: Sapphire swap {player.NetId} {relic.Id}: restore {plan.Restore.Count}, remove {plan.Remove.Count}, gold {plan.GoldToLose}, HP {plan.MaxHpToLose}");
	}
}
