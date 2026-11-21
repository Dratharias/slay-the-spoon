using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Characters;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Relics.Sts1;
using SlaySpoon.Relics.Watcher;
using SlaySpoon.Ui;

namespace SlaySpoon.Relics;

/// <summary>
/// The StS1 boss chest of <see cref="SpoonBossRelics"/>. The act 2 and act 3 Ancients get one more initial option;
/// taking it replaces the Ancient's gift with a chest of <see cref="BossChestRule.Size"/> StS1 boss relics: the 11 Darv
/// natives (with Darv's act and Pandora's Box rules), the 8 shared ports and the takers' character boss relics.
/// Ancients are per player, so the chest waits until every player has made their first choice; the takers then vote
/// through the vanilla synchronized "Choose a Relic" screen and <see cref="BossChestRule.Resolve"/> settles ties with
/// the vanilla rock-paper-scissors fight. Every roll uses one stream seeded from the run seed and the act, the same on
/// every client, so vanilla seeds are untouched. A relic any taker already owns is left out.
/// </summary>
public static class BossChest
{
	public const string TextKey = "SPOON_BOSS_RELICS.INITIAL";
	private const string Table = "modifiers";

	private static readonly Action<EventModel, LocString> SetEventFinished =
		AccessTools.MethodDelegate<Action<EventModel, LocString>>(AccessTools.Method(typeof(EventModel), "SetEventFinished"));
	private static readonly Action<EventModel, LocString, IEnumerable<EventOption>> SetEventState =
		AccessTools.MethodDelegate<Action<EventModel, LocString, IEnumerable<EventOption>>>(
			AccessTools.Method(typeof(EventModel), "SetEventState"));
	private static readonly Action<AncientEventModel> UpdateRunHistory =
		AccessTools.MethodDelegate<Action<AncientEventModel>>(AccessTools.Method(typeof(AncientEventModel), "UpdateRunHistory"));

	private static Room? _room;

	private static RelicModel[] Natives => new RelicModel[]
	{
		ModelDb.Relic<Astrolabe>(), ModelDb.Relic<BlackStar>(), ModelDb.Relic<CallingBell>(), ModelDb.Relic<EmptyCage>(),
		ModelDb.Relic<PandorasBox>(), ModelDb.Relic<RunicPyramid>(), ModelDb.Relic<SneckoEye>(), ModelDb.Relic<Ectoplasm>(),
		ModelDb.Relic<Sozu>(), ModelDb.Relic<PhilosophersStone>(), ModelDb.Relic<VelvetChoker>(),
	};

	private static RelicModel[] SharedPorts => new RelicModel[]
	{
		ModelDb.Relic<Sts1BustedCrown>(), ModelDb.Relic<Sts1CoffeeDripper>(), ModelDb.Relic<Sts1CursedKey>(),
		ModelDb.Relic<Sts1FusionHammer>(), ModelDb.Relic<Sts1RunicDome>(), ModelDb.Relic<Sts1SacredBark>(),
		ModelDb.Relic<Sts1SlaversCollar>(), ModelDb.Relic<Sts1TinyHouse>(),
	};

	private static RelicModel[] CharacterRelics(CharacterModel character) => character switch
	{
		Ironclad => new RelicModel[] { ModelDb.Relic<Sts1MarkOfPain>(), ModelDb.Relic<Sts1RunicCube>() },
		Silent => new RelicModel[] { ModelDb.Relic<Sts1HoveringKite>(), ModelDb.Relic<Sts1RingOfTheSerpent>(), ModelDb.Relic<Sts1WristBlade>() },
		Defect => new RelicModel[] { ModelDb.Relic<Sts1FrozenCore>(), ModelDb.Relic<Sts1Inserter>(), ModelDb.Relic<Sts1NuclearBattery>() },
		Sts1Watcher => new RelicModel[] { ModelDb.Relic<Sts1HolyWater>(), ModelDb.Relic<Sts1VioletLotus>() },
		_ => Array.Empty<RelicModel>(),
	};

	private static LocString Text(string key) => new(Table, $"SPOON_BOSS_RELICS.{key}");

	internal static EventOption Option(AncientEventModel ancient, Player player) =>
		new(ancient, () => Open(ancient, player), Text("option_title"),
			Text(player.RunState.Players.Count > 1 ? "option_description_coop" : "option_description"), TextKey,
			Array.Empty<IHoverTip>());

	internal static void Begin(EventSynchronizer sync, EventModel canonical, bool isPrefinished)
	{
		if (canonical is not AncientEventModel || isPrefinished || sync.Events.Count == 0
			|| !sync.Events[0].Owner!.RunState.Modifiers.Any(m => m is SpoonBossRelics))
		{
			_room = null;
			return;
		}
		_room = new Room(sync.Events[0].Owner!.RunState);
		foreach (EventModel ev in sync.Events)
			if (ev.IsFinished && ev.Owner is { } owner) _room.Decide(owner, taker: false);
	}

	internal static void Chosen(Player player, EventModel ev, int optionIndex)
	{
		if (_room == null || ev is not AncientEventModel || optionIndex < 0 || optionIndex >= ev.CurrentOptions.Count) return;
		_room.Decide(player, ev.CurrentOptions[optionIndex].TextKey == TextKey);
	}

	private static async Task Open(AncientEventModel ancient, Player player)
	{
		Room? room = _room;
		if (room == null)
		{
			SpoonLog.Info("boss chest: no room state, closing the Ancient");
			Finish(ancient, Text("empty"));
			return;
		}
		if (!room.AllDecided.Task.IsCompleted)
		{
			// An event with no options counts as finished: the room would show Proceed (leaving without the chest)
			// and the final SetEventFinished would throw. A locked option keeps the page open while waiting.
			SetEventState(ancient, Text("waiting"), [new EventOption(ancient, null, Text("option_title"), Text("waiting"),
				TextKey, Array.Empty<IHoverTip>())]);
			await room.AllDecided.Task;
		}
		List<RelicModel> relics = room.Relics();
		if (relics.Count == 0)
		{
			room.Vote(player, null);
			Finish(ancient, Text("none"));
			return;
		}
		if (LocalContext.IsMe(player)) await ChestOpeningOverlay.Play(player.RunState.Act);
		RelicModel? chosen = await RelicSelectCmd.FromChooseARelicScreen(player, relics);
		int index = chosen == null ? -1 : relics.IndexOf(chosen);
		room.Vote(player, index < 0 ? null : index);
		BossChestAward award = (await room.Resolved.Task)[player.NetId];
		LocString result = Text(award.Outcome switch
		{
			BossChestOutcome.Only => "only",
			BossChestOutcome.WonFight => "won",
			BossChestOutcome.Consolation => "consolation",
			BossChestOutcome.Empty => "lost",
			_ => "skipped",
		});
		if (award.Relic is { } won)
		{
			RelicModel relic = relics[won];
			result.Add("Relic", relic.Title.GetFormattedText());
			await RelicCmd.Obtain(relic.ToMutable(), player);
		}
		Finish(ancient, result);
	}

	private static void Finish(AncientEventModel ancient, LocString description)
	{
		UpdateRunHistory(ancient);
		SetEventFinished(ancient, description);
	}

	/// <summary>One Ancient visit: who took the chest, what it holds, the votes and the result.</summary>
	private sealed class Room
	{
		private readonly IRunState _run;
		private readonly Dictionary<ulong, bool> _decided = new();
		private readonly Dictionary<ulong, int?> _votes = new();
		private readonly Rng _rng;
		private List<RelicModel>? _relics;

		public Room(IRunState run)
		{
			_run = run;
			_rng = new Rng(run.Rng.Seed + (uint)run.CurrentActIndex, "SPOON_BOSS_RELICS");
		}

		public TaskCompletionSource AllDecided { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public TaskCompletionSource<Dictionary<ulong, BossChestAward>> Resolved { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		private List<Player> Takers => _run.Players.Where(p => _decided.GetValueOrDefault(p.NetId)).ToList();

		public void Decide(Player player, bool taker)
		{
			if (!_decided.TryAdd(player.NetId, taker)) return;
			if (taker) SpoonLog.Info($"boss chest: player {player.NetId} takes the chest");
			if (_run.Players.All(p => _decided.ContainsKey(p.NetId))) AllDecided.TrySetResult();
		}

		/// <summary>The chest's relics, drawn once all players have decided (the takers are known then).</summary>
		public List<RelicModel> Relics()
		{
			if (_relics != null) return _relics;
			List<Player> takers = Takers;
			int act = _run.CurrentActIndex;
			bool clearsDeck = _run.Modifiers.Any(m => m.ClearsPlayerDeck);
			var characters = takers.Select(p => p.Character.Id).ToList();
			var candidates = Natives.Where(r => BossChestRule.NativeAllowed(r.Id.Entry, act, clearsDeck)).Concat(SharedPorts)
				.Concat(takers.Select(p => p.Character).DistinctBy(c => c.Id)
					.Where(c => BossChestRule.CharacterRelicsShared(characters, c.Id)).SelectMany(CharacterRelics))
				.Where(r => !takers.Any(p => p.Relics.Any(owned => owned.Id == r.Id)))
				.ToList();
			List<int> order = Enumerable.Range(0, candidates.Count).ToList().StableShuffle(_rng);
			_relics = order.Take(BossChestRule.Size).Select(i => candidates[i]).ToList();
			SpoonLog.Info($"boss chest: act {act + 1}, takers {string.Join(", ", takers.Select(p => p.NetId))}, "
				+ $"relics {string.Join(", ", _relics.Select(r => r.Id.Entry))}");
			return _relics;
		}

		public void Vote(Player player, int? relic)
		{
			_votes[player.NetId] = relic;
			List<Player> takers = Takers;
			if (Resolved.Task.IsCompleted || !takers.All(p => _votes.ContainsKey(p.NetId))) return;
			var votes = takers.ToDictionary(p => takers.IndexOf(p), p => _votes[p.NetId]);
			var moves = Enum.GetValues<RelicPickingFightMove>();
			List<BossChestAward> awards = BossChestRule.Resolve(votes, Relics().Count,
				(voters, index) =>
				{
					var fight = RelicPickingResult.GenerateRelicFight(voters.Select(v => takers[v]).ToList(), Relics()[index],
						() => _rng.NextItem(moves));
					return takers.IndexOf(fight.player!);
				},
				left => left.StableShuffle(_rng));
			Resolved.TrySetResult(awards.ToDictionary(a => takers[a.Player].NetId, a => a));
		}
	}
}

[HarmonyPatch(typeof(AncientEventModel), "GenerateInitialOptionsWrapper")]
public static class BossChestOptionPatch
{
	public static void Postfix(AncientEventModel __instance, IReadOnlyList<EventOption> __result)
	{
		if (__result is not List<EventOption> options || __instance.Owner is not { } owner) return;
		IRunState run = owner.RunState;
		if (!run.Modifiers.Any(m => m is SpoonBossRelics) || !BossChestRule.Offers(run.CurrentActIndex, __instance is MegaCrit.Sts2.Core.Models.Events.Neow)) return;
		options.Add(BossChest.Option(__instance, owner));
	}
}

[HarmonyPatch(typeof(EventSynchronizer), nameof(EventSynchronizer.BeginEvent))]
public static class BossChestBeginPatch
{
	public static void Postfix(EventSynchronizer __instance, EventModel canonicalEvent, bool isPrefinished) =>
		BossChest.Begin(__instance, canonicalEvent, isPrefinished);
}

[HarmonyPatch(typeof(EventSynchronizer), "ChooseOptionForEvent")]
public static class BossChestChoicePatch
{
	public static void Prefix(EventSynchronizer __instance, Player player, int optionIndex) =>
		BossChest.Chosen(player, __instance.GetEventForPlayer(player), optionIndex);
}
