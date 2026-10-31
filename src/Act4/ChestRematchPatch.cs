using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using Vote = MegaCrit.Sts2.Core.Multiplayer.Game.TreasureRoomRelicSynchronizer.PlayerVote;

namespace SlaySpoon.Act4;

/// <summary>
/// An intermediate rematch round, for the chest UI: each award with its relic index, the players who vote again, and
/// whether the last of them may take the key.
/// </summary>
internal sealed record RematchRoundView(List<(RelicPickingResult Result, int Index)> Results, int Round,
	List<Player> Rematch, bool KeyOffered);

/// <summary>
/// Sapphire chests (<see cref="ChestRematchRule"/>): while an Act 4 run still lacks the Sapphire Key, the shared chest
/// replaces the vanilla consolation prize with rematch rounds, and the last player left may take the key instead of a
/// relic. Votes still travel as the vanilla <see cref="PickRelicAction"/>, the key as the index past the last relic, and
/// every peer resolves the rounds in action-queue order with the chest's own <see cref="Rng"/>. Other chests stay vanilla.
/// </summary>
internal static class ChestRematchPatch
{
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, IPlayerCollection> Players =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, IPlayerCollection>("_playerCollection");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, ulong> LocalId =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, ulong>("_localPlayerId");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, ActionQueueSynchronizer> Queue =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, ActionQueueSynchronizer>("_actionQueueSynchronizer");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, Rng> ChestRng =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, Rng>("_rng");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, List<RelicModel>?> Current =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, List<RelicModel>?>("_currentRelics");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, List<Vote>> Votes =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, List<Vote>>("_votes");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, Vote?> Predicted =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, Vote?>("_predictedVote");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, bool> SoloSkipped =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, bool>("_singleplayerSkipped");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, Action?> VotesChanged =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, Action?>("VotesChanged");
	private static readonly AccessTools.FieldRef<TreasureRoomRelicSynchronizer, Action<List<RelicPickingResult>>?> Awarded =
		AccessTools.FieldRefAccess<TreasureRoomRelicSynchronizer, Action<List<RelicPickingResult>>?>("RelicsAwarded");

	// Game-logic state, the same on every peer.
	private static TreasureRoomRelicSynchronizer? _sync;
	private static ChestRematchRule? _rule;
	private static readonly List<RelicPickingResult> Earlier = [];

	/// <summary>Raised on every peer after a round that leaves the chest open.</summary>
	internal static event Action<RematchRoundView>? RoundResolved;
	/// <summary>Raised when the key offer may have changed: a new chest, a round, or the chest's end.</summary>
	internal static event Action? OfferChanged;

	private static ChestRematchRule? RuleFor(TreasureRoomRelicSynchronizer sync) => _sync == sync ? _rule : null;

	/// <summary>The key's vote index, while <paramref name="player"/> is the one it is offered to.</summary>
	internal static int? KeyOfferFor(Player player)
	{
		if (_sync is not { } sync || _rule is not { KeyOffered: true } rule) return null;
		int slot = Players(sync).GetPlayerSlotIndex(player);
		return rule.IsActive(slot) ? rule.KeyIndex : null;
	}

	internal static bool IsRematchChest => _rule is { Finished: false };

	private static void Clear()
	{
		_sync = null;
		_rule = null;
		Earlier.Clear();
	}

	[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking))]
	private static class BeginPatch
	{
		private static void Postfix(TreasureRoomRelicSynchronizer __instance)
		{
			Clear();
			if (Current(__instance) is not { Count: > 0 } relics || Players(__instance) is not IRunState run) return;
			if (SpoonAct4.Of(run) is not { } act4 || act4.Keys.Has(Act4Key.Sapphire)) return;
			_sync = __instance;
			_rule = new ChestRematchRule(run.Players.Count, relics.Count, keyAvailable: true);
			SpoonLog.Info($"act4: Sapphire chest, {run.Players.Count} players, {relics.Count} relics: rematch rounds");
			OfferChanged?.Invoke();
		}
	}

	/// <summary>Local clicks: votes the current round refuses are dropped here; the key vote skips the vanilla relic log.</summary>
	[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.PickRelicLocally))]
	private static class PickLocallyPatch
	{
		private static bool Prefix(TreasureRoomRelicSynchronizer __instance, int? index)
		{
			if (RuleFor(__instance) is not { Finished: false } rule) return true;
			var players = Players(__instance);
			if (players.GetPlayer(LocalId(__instance)) is not { } me) return true;
			int slot = players.GetPlayerSlotIndex(me);
			// Solo skips through the proceed button: vanilla handles it.
			if (index == null && players.Players.Count == 1) return true;
			if (ChestRematchView.Busy || !rule.CanVote(slot, index)) return false;
			if (index != rule.KeyIndex) return true;
			Predicted(__instance) = new Vote { index = index, voteReceived = true };
			Queue(__instance).RequestEnqueue(new PickRelicAction(me, index));
			VotesChanged(__instance)?.Invoke();
			return false;
		}
	}

	/// <summary>Every peer runs this through the action queue.</summary>
	[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.OnPicked))]
	private static class OnPickedPatch
	{
		private static bool Prefix(TreasureRoomRelicSynchronizer __instance, Player player, int? index)
		{
			if (RuleFor(__instance) is not { } rule) return true;
			var players = Players(__instance);
			if (index == null && players.Players.Count == 1) return true;
			if (rule.Finished || Current(__instance) == null) return false;
			int slot = players.GetPlayerSlotIndex(player);
			if (!rule.CanVote(slot, index))
			{
				SpoonLog.Info($"act4: Sapphire chest round {rule.Round}: vote {index} from {player.NetId} ignored");
				return false;
			}
			var vote = Votes(__instance)[slot];
			vote.index = index;
			vote.voteReceived = true;
			VotesChanged(__instance)?.Invoke();
			ResolveReadyRounds(__instance, rule);
			return false;
		}
	}

	private static void ResolveReadyRounds(TreasureRoomRelicSynchronizer sync, ChestRematchRule rule)
	{
		var players = Players(sync);
		var votes = Votes(sync);
		var relics = Current(sync)!;
		var rng = ChestRng(sync);
		while (!rule.Finished && rule.Active.All(p => votes[p].voteReceived))
		{
			Predicted(sync) = null;
			var fights = new Dictionary<int, RelicPickingResult>();
			var moves = Enum.GetValues<RelicPickingFightMove>();
			var round = rule.Resolve(rule.Active.ToDictionary(p => p, p => votes[p].index), (voters, relic) =>
			{
				var fight = RelicPickingResult.GenerateRelicFight(voters.Select(p => players.Players[p]).ToList(), relics[relic],
					() => rng.NextItem(moves));
				fights[relic] = fight;
				return players.GetPlayerSlotIndex(fight.player!);
			});
			var results = round.Awards.Select(a => (a.Fought ? fights[a.Relic] : new RelicPickingResult
			{
				type = RelicPickingResultType.OnlyOnePlayerVoted,
				relic = relics[a.Relic],
				player = players.Players[a.Player],
			}, a.Relic)).ToList();
			if (round.KeyTakenBy is { } taker)
			{
				var run = (IRunState)players;
				bool taken = SpoonAct4.Of(run)?.Keys.Take(Act4Key.Sapphire) == true;
				SpoonLog.Info($"act4: Sapphire Key {(taken ? "taken" : "already held")} by {players.Players[taker].NetId}");
			}
			SpoonLog.Info($"act4: Sapphire chest round {rule.Round - (round.Finished ? 0 : 1)}: "
				+ string.Join(", ", round.Awards.Select(a => $"{players.Players[a.Player].NetId}<-{relics[a.Relic].Id}{(a.Fought ? " (fight)" : "")}")));
			if (round.Finished)
			{
				Finish(sync, rule, round, results.Select(r => r.Item1));
				return;
			}
			Earlier.AddRange(results.Select(r => r.Item1));
			foreach (int p in rule.Active)
			{
				votes[p].index = null;
				votes[p].voteReceived = false;
			}
			RoundResolved?.Invoke(new RematchRoundView(results, rule.Round,
				rule.Active.Select(p => players.Players[p]).ToList(), rule.KeyOffered));
			OfferChanged?.Invoke();
			if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer && players.Players.Count > 1)
			{
				var me = players.GetPlayer(LocalId(sync));
				foreach (int p in rule.Active.Where(p => players.Players[p] != me))
				{
					var choices = rule.Choices();
					votes[p].index = choices[rng.NextInt(choices.Count)];
					votes[p].voteReceived = true;
				}
			}
			VotesChanged(sync)?.Invoke();
		}
	}

	/// <summary>
	/// Hands every award to the vanilla chest animation at once. Earlier rounds count as single votes there, so their
	/// fights are not replayed; relics nobody took are skipped.
	/// </summary>
	private static void Finish(TreasureRoomRelicSynchronizer sync, ChestRematchRule rule, RematchRound round,
		IEnumerable<RelicPickingResult> last)
	{
		var players = Players(sync);
		var relics = Current(sync)!;
		var results = Earlier.Select(r => new RelicPickingResult
		{
			type = RelicPickingResultType.OnlyOnePlayerVoted,
			relic = r.relic,
			player = r.player,
		}).Concat(last).ToList();
		if (round.Consolation is { } prize)
			results.Add(new RelicPickingResult
			{
				type = RelicPickingResultType.ConsolationPrize,
				relic = relics[prize.Relic],
				player = players.Players[prize.Player],
			});
		results.AddRange(rule.Remaining.Select(i => new RelicPickingResult
		{
			type = RelicPickingResultType.Skipped,
			relic = relics[i],
			player = null,
		}));
		Clear();
		OfferChanged?.Invoke();
		Awarded(sync)?.Invoke(results);
		// EndRelicVoting.
		Current(sync) = null;
		SoloSkipped(sync) = false;
	}

	[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.OnRoomExited))]
	private static class RoomExitPatch
	{
		private static void Postfix(TreasureRoomRelicSynchronizer __instance)
		{
			if (_sync != __instance) return;
			Clear();
			OfferChanged?.Invoke();
		}
	}

	[HarmonyPatch(typeof(RunManager), "InitializeShared")]
	private static class RunStartPatch
	{
		private static void Postfix() => Clear();
	}
}
