using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Act4;

/// <summary>
/// Chest UI for <see cref="ChestRematchPatch"/>: plays each intermediate round like the vanilla award (hands grab, fights
/// over the relic), then frees the losers' hands for the rematch and shows the Sapphire Key to the last player. The
/// final award is the vanilla animation, queued after the rounds still playing.
/// </summary>
internal static class ChestRematchView
{
	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>> Holders =
		AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>>("_holdersInUse");
	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, NHandImageCollection> Hands =
		AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, NHandImageCollection>("_hands");
	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, Control> Backstop =
		AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, Control>("_fightBackstop");
	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, IRunState> Run =
		AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, IRunState>("_runState");
	private static readonly Func<NTreasureRoomRelicCollection, List<RelicPickingResult>, Task> AnimateAwards =
		AccessTools.MethodDelegate<Func<NTreasureRoomRelicCollection, List<RelicPickingResult>, Task>>(
			AccessTools.Method(typeof(NTreasureRoomRelicCollection), "AnimateRelicAwards"));

	private static NTreasureRoomRelicCollection? _collection;
	private static Task _pending = Task.CompletedTask;
	private static int _running;
	private static CancellationTokenSource _cts = new();
	private static SapphireKeyView? _key;
	private static Label? _label;
	private static bool _opened;

	/// <summary>True while a round still plays on this peer: local clicks wait for the rematch.</summary>
	internal static bool Busy => _running > 0;

	[HarmonyPatch(typeof(NTreasureRoomRelicCollection), nameof(NTreasureRoomRelicCollection._Ready))]
	private static class ReadyPatch
	{
		private static void Postfix(NTreasureRoomRelicCollection __instance)
		{
			_collection = __instance;
			_pending = Task.CompletedTask;
			_running = 0;
			_cts = new CancellationTokenSource();
			_opened = false;
			_key = SapphireKeyView.Add(__instance, TakeKey);
			_label = new Label
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				Visible = false,
			};
			_label.AddThemeFontSizeOverride("font_size", 34);
			_label.AddThemeColorOverride("font_outline_color", Colors.Black);
			_label.AddThemeConstantOverride("outline_size", 10);
			__instance.AddChild(_label);
			_label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
			_label.Position = new Vector2(_label.Position.X, 140f);
			_label.GrowHorizontal = Control.GrowDirection.Both;

			void OnRound(RematchRoundView round) => Chain(() => AnimateRound(__instance, round));
			void OnOffer() => Refresh();
			ChestRematchPatch.RoundResolved += OnRound;
			ChestRematchPatch.OfferChanged += OnOffer;
			__instance.TreeExiting += () =>
			{
				ChestRematchPatch.RoundResolved -= OnRound;
				ChestRematchPatch.OfferChanged -= OnOffer;
				_cts.Cancel();
				if (_collection == __instance) _collection = null;
				_key = null;
				_label = null;
			};
		}
	}

	/// <summary>The relics are on screen once the chest opens: only then does the key show.</summary>
	[HarmonyPatch(typeof(NTreasureRoomRelicCollection), nameof(NTreasureRoomRelicCollection.InitializeRelics))]
	private static class OpenPatch
	{
		private static void Postfix(NTreasureRoomRelicCollection __instance)
		{
			if (__instance != _collection) return;
			_opened = true;
			Refresh();
		}
	}

	/// <summary>The final award waits for the rounds still playing on this peer.</summary>
	[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "OnRelicsAwarded")]
	private static class AwardedPatch
	{
		private static bool Prefix(NTreasureRoomRelicCollection __instance, List<RelicPickingResult> results)
		{
			if (__instance != _collection || !Busy) return true;
			Chain(() => AnimateAwards(__instance, results));
			return false;
		}
	}

	/// <summary>A relic an earlier round already put in its winner's hand is not grabbed again by the final award.</summary>
	[HarmonyPatch(typeof(NHandImage), nameof(NHandImage.GrabRelic))]
	private static class GrabPatch
	{
		private static bool Prefix(NHandImage __instance, NTreasureRoomRelicHolder holder, ref Task __result)
		{
			if (holder.GetParent() != __instance) return true;
			__result = Task.CompletedTask;
			return false;
		}
	}

	private static void Chain(Func<Task> next)
	{
		var previous = _pending;
		_running++;
		Refresh();
		_pending = TaskHelper.RunSafely(Run());
		async Task Run()
		{
			try
			{
				try { await previous; }
				catch (Exception e) { SpoonLog.Error($"act4: chest round animation failed: {e}"); }
				await next();
			}
			finally
			{
				_running--;
				Refresh();
			}
		}
	}

	private static bool TakeKey()
	{
		var sync = RunManager.Instance.TreasureRoomRelicSynchronizer;
		if (_collection == null || Busy || LocalContext.GetMe(Run(_collection)) is not { } me
			|| ChestRematchPatch.KeyOfferFor(me) is not { } index) return false;
		sync.PickRelicLocally(index);
		return true;
	}

	private static void Refresh()
	{
		if (_collection is not { } collection || !GodotObject.IsInstanceValid(collection)) return;
		var me = LocalContext.GetMe(Run(collection));
		bool open = _opened && ChestRematchPatch.IsRematchChest && !Busy && me != null;
		bool offered = open && ChestRematchPatch.KeyOfferFor(me!) != null;
		var beside = Holders(collection).Where(h => h.Visible && h.GetParent() is not NHandImage)
			.OrderBy(h => h.GlobalPosition.X).LastOrDefault();
		_key?.Show(offered, new LocString("merchant_room", "SPOON_SAPPHIRE_KEY_TIP").GetFormattedText(), beside);
	}

	private static void ShowLabel(string? key)
	{
		if (_label == null) return;
		_label.Visible = key != null;
		if (key != null) _label.Text = new LocString("merchant_room", key).GetFormattedText();
	}

	private static async Task AnimateRound(NTreasureRoomRelicCollection collection, RematchRoundView round)
	{
		if (!GodotObject.IsInstanceValid(collection)) return;
		var token = _cts.Token;
		var holders = Holders(collection);
		var hands = Hands(collection);
		var backstop = Backstop(collection);
		var run = Run(collection);
		ShowLabel(null);
		foreach (var holder in holders) holder.SetFocusMode(Control.FocusModeEnum.None);
		hands.BeforeRelicsAwarded();
		var grabs = new List<Task>();
		foreach (var (result, index) in round.Results.OrderBy(r => r.Result.type))
		{
			var holder = holders.First(h => h.Index == index);
			holder.AnimateAwayVotes();
			if (result.type == RelicPickingResultType.FoughtOver)
			{
				// The vanilla fight sequence (NTreasureRoomRelicCollection.AnimateRelicAwards).
				holder.ZIndex = 1;
				backstop.Visible = true;
				var tween = collection.CreateTween();
				tween.TweenProperty(holder, "global_position", (backstop.Size - holder.Size) * 0.5f, 0.25)
					.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
				tween.TweenProperty(backstop, "modulate:a", 1f, 0.25);
				hands.BeforeFightStarted(result.fight!.playersInvolved);
				await tween.AwaitFinished(token);
				await Cmd.Wait(1f, token);
				await hands.DoFight(result, holder);
				tween = collection.CreateTween();
				tween.TweenProperty(backstop, "modulate:a", 0f, 0.25);
				await tween.AwaitFinished(token);
				backstop.Visible = false;
				holder.ZIndex = 0;
			}
			else if (hands.GetHand(result.player!.NetId) is { } hand)
			{
				grabs.Add(TaskHelper.RunSafely(hand.GrabRelic(holder)));
				await Cmd.Wait(0.25f, token);
			}
		}
		await Task.WhenAll(grabs);
		foreach (var (_, index) in round.Results) holders.First(h => h.Index == index).Disable();

		// Winners keep their relic in hand; everyone else points again.
		var winners = round.Results.Select(r => r.Result.player).ToHashSet();
		foreach (var player in run.Players.Where(p => !winners.Contains(p)))
			if (hands.GetHand(player.NetId) is { } hand)
			{
				hand.SetFrozenForRelicAwards(false);
				if (LocalContext.IsMe(player)) hand.Modulate = Colors.White;
			}

		var me = LocalContext.GetMe(run);
		var free = holders.Where(h => h.Visible && h.GetParent() is not NHandImage).ToList();
		bool rematching = me != null && free.Count > 0 && round.Rematch.Contains(me);
		for (int i = 0; i < free.Count; i++)
		{
			free[i].SetFocusMode(Control.FocusModeEnum.All);
			free[i].FocusNeighborLeft = free[(i + free.Count - 1) % free.Count].GetPath();
			free[i].FocusNeighborRight = free[(i + 1) % free.Count].GetPath();
		}
		ShowLabel(!rematching ? "SPOON_CHEST_REMATCH_WAIT" : round.KeyOffered ? "SPOON_CHEST_REMATCH_KEY" : "SPOON_CHEST_REMATCH");
		if (rematching && NControllerManager.Instance?.IsUsingController == true) free[0].TryGrabFocus();
		SpoonLog.Info($"act4: chest round {round.Round - 1} shown, rematch {(rematching ? "for you" : "for the others")}");
	}

	/// <summary>Hides the rematch line once the chest is done.</summary>
	[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "AnimateRelicAwards")]
	private static class FinalPatch
	{
		private static void Prefix(NTreasureRoomRelicCollection __instance)
		{
			if (__instance == _collection) ShowLabel(null);
		}
	}
}
