using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.CardSets;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Folie;
using SlaySpoon.Ui;

namespace SlaySpoon.Conductor;

/// <summary>
/// Conductor on combat rewards: every client generates every player's set as vanilla does, a seeded Conductor (never
/// the previous one) picks a recipient for each card, potion and relic reward through a synced choice, and the rewards
/// move to their recipients' sets before the sets are offered. Card rewards keep the cards their first owner was
/// offered. Gold stays with each player: the game already gives every player the same amount.
/// </summary>
public static class ConductorRewards
{
	public static async Task OfferRoomEnd(CombatRoom room, SpoonConductor conductor)
	{
		var sets = new List<RewardsSet>();
		foreach (Player player in room.CombatState.Players)
			sets.Add(await RewardsCmd.GenerateForRoomEnd(player, room));
		IRunState run = room.CombatState.Players[0].RunState;
		try
		{
			await Distribute(run, conductor, sets);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"conductor: hand-out failed, rewards stay with their owners: {e}");
		}
		foreach (RewardsSet set in sets)
			_ = TaskHelper.RunSafely(set.Offer());
	}

	private static bool Movable(Reward reward) => reward switch
	{
		CardReward card => card.IsPopulated && card.Cards.Any(),
		PotionReward potion => potion.Potion != null,
		RelicReward relic => relic.Relic != null,
		_ => false,
	};

	private static async Task Distribute(IRunState run, SpoonConductor conductor, IReadOnlyList<RewardsSet> sets)
	{
		var living = sets.Where(s => !s.Player.Creature.IsDead).ToList();
		if (!ConductorRule.Applies(living.Count)) return;
		var players = living.Select(s => s.Player).ToList();
		int previous = players.FindIndex(p => run.Players.ToList().IndexOf(p) == conductor.SpoonConductorPrevious);
		var rng = new Rng(run.Rng.Seed, ConductorRule.Stream(run.TotalFloor));
		int slot = ConductorRule.Pick(players.Count, previous, rng.NextInt(ConductorRule.Rolls(players.Count, previous)));
		Player chief = players[slot];
		conductor.SpoonConductorPrevious = run.Players.ToList().IndexOf(chief);
		SpoonLog.Info($"conductor: {chief.NetId} hands out the rewards");

		var moves = new List<(RewardsSet from, Reward reward, Player to)>();
		var context = new BlockingPlayerChoiceContext();
		foreach (var set in living)
		{
			foreach (var reward in set.Rewards.Where(Movable).ToList())
			{
				int pick = await SyncedChoice.Choose(context, chief, () => ConductorPopup.Ask(Describe(set.Player, reward), players, players.IndexOf(set.Player)));
				Player to = players[Math.Clamp(pick, 0, players.Count - 1)];
				if (to != set.Player) moves.Add((set, reward, to));
			}
		}
		foreach (var (from, reward, to) in moves)
		{
			Reward? moved = reward switch
			{
				CardReward card => FolieRewards.Rebuild(run, card, to),
				PotionReward { Potion: { } potion } => new PotionReward(potion, to),
				RelicReward { Relic: { } relic } when to.Relics.All(r => r.Id != relic.Id) => new RelicReward(relic, to),
				_ => null,
			};
			if (moved == null) continue;
			from.Rewards.Remove(reward);
			RewardsSet target = living.First(x => x.Player == to);
			target.Rewards.Add(moved);
			target.Rewards.Sort((x, y) => x.RewardsSetIndex.CompareTo(y.RewardsSetIndex));
			SpoonLog.Info($"conductor: {reward.GetType().Name} of {from.Player.NetId} goes to {to.NetId}");
		}
	}

	private static string Describe(Player owner, Reward reward)
	{
		string what = reward is CardReward card
			? string.Join(", ", card.Cards.Select(c => c.Title))
			: reward.Description.GetFormattedText();
		var text = new LocString("modifiers", "SPOON_CONDUCTOR.reward");
		text.Add("Name", ConductorPopup.Name(owner));
		text.Add("Reward", what);
		return text.GetFormattedText();
	}
}

/// <summary>The Conductor's recipient picker: one button per living player, not cancellable.</summary>
public static class ConductorPopup
{
	public static string Name(Player player) =>
		PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId);

	public static Task<int> Ask(string reward, IReadOnlyList<Player> players, int fallback)
	{
		var done = new TaskCompletionSource<int>();
		var popup = NativeTradePopup.Open(new LocString("modifiers", "SPOON_CONDUCTOR.ask").GetFormattedText());
		if (popup == null) return Task.FromResult(fallback);
		NativeTradePopup.Label(popup.Rows, reward);
		var row = NativeTradePopup.Actions(popup.Footer);
		for (int i = 0; i < players.Count; i++)
		{
			int index = i;
			NativeTradePopup.Button(row, Name(players[i]), () =>
			{
				if (done.TrySetResult(index)) popup.Close();
			});
		}
		popup.Closed += () => done.TrySetResult(fallback);
		return done.Task;
	}
}

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OfferRoomEndRewards))]
public static class ConductorCombatRewardsPatch
{
	public static bool Prefix(CombatRoom __instance, ref Task __result)
	{
		if (__instance.CombatState.Players.FirstOrDefault()?.RunState is not { } run
		    || SpoonFolieADeux.IsActive(run) || SpoonConductor.Of(run) is not { } conductor) return true;
		__result = ConductorRewards.OfferRoomEnd(__instance, conductor);
		return false;
	}
}
