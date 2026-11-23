using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
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
/// offered, unless the Conductor clicked one of them: that card goes straight into the recipient's deck (owner,
/// 2026-11-23). Gold stays with each player: the game already gives every player the same amount.
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

		var offered = living.SelectMany(set => set.Rewards.Where(Movable).Select(reward => (set, reward))).ToList();
		if (offered.Count == 0) return;
		var owners = offered.Select(o => players.IndexOf(o.set.Player)).ToList();
		var context = new BlockingPlayerChoiceContext();
		List<int> picks = await SyncedChoice.ChooseMany(context, chief,
			() => ConductorOverlay.Ask(offered.Select(o => (o.set.Player, o.reward)).ToList(), players));
		int[] recipients = ConductorRule.Recipients(picks, owners, players.Count);
		int[] cardPicks = ConductorRule.CardPicks(picks,
			offered.Select(o => o.reward is CardReward c ? c.Cards.Count() : 0).ToList(), players.Count);
		var moves = new List<(RewardsSet from, Reward reward, Player to)>();
		var direct = new List<(RewardsSet from, CardReward reward, CardModel card, Player to)>();
		for (int i = 0; i < offered.Count; i++)
		{
			Player to = players[recipients[i]];
			if (offered[i].reward is CardReward cardReward && cardPicks[i] >= 0)
				direct.Add((offered[i].set, cardReward, cardReward.Cards.ElementAt(cardPicks[i]), to));
			else if (to != offered[i].set.Player)
				moves.Add((offered[i].set, offered[i].reward, to));
		}
		foreach (var (from, reward, card, to) in direct)
		{
			from.Rewards.Remove(reward);
			if (to != from.Player)
			{
				run.RemoveCard(card);
				run.AddCard(card, to);
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
			SpoonLog.Info($"conductor: {card.Id.Entry} from {from.Player.NetId}'s card reward goes to {to.NetId}'s deck");
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
