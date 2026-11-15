using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Matriochka;

namespace SlaySpoon.Folie;

/// <summary>
/// Folie à deux on reward sets: every client generates every player's set as vanilla does, the first card reward of
/// each living player becomes their stake, the chest decides who takes which, and each winner's set gets the reward
/// rebuilt for them (the cards change owner and keep their character, upgrades and enchantments). Then the sets are
/// offered as vanilla offers them.
/// </summary>
public static class FolieRewards
{
	private static readonly Func<CardReward, CardCreationOptions> RerollOptions =
		AccessTools.MethodDelegate<Func<CardReward, CardCreationOptions>>(AccessTools.PropertyGetter(typeof(CardReward), "RerollOptions"));

	/// <summary>Runs the chest over the sets' card rewards and moves them to their winners. Solo or no stake: nothing.</summary>
	public static async Task Redistribute(IRunState run, string key, IReadOnlyList<RewardsSet> sets)
	{
		var living = sets.Where(s => !s.Player.Creature.IsDead).ToList();
		if (living.Count < 2) return;
		var stakes = living
			.Select(set => (set, reward: set.Rewards.OfType<CardReward>().FirstOrDefault(r => r.IsPopulated && r.Cards.Any())))
			.Where(x => x.reward != null)
			.Select(x => (x.set, reward: x.reward!))
			.ToList();
		if (stakes.Count == 0) return;
		var bidders = living.Select(s => s.Player).ToList();
		FolieOutcome outcome = await FolieChest.Run(run, key, bidders,
			stakes.Select(x => new FolieStake(x.set.Player, x.reward.Cards.ToList())).ToList(), consolation: false);

		for (int s = 0; s < stakes.Count; s++)
		{
			var (set, reward) = stakes[s];
			FolieAward? award = outcome.Awards.Where(a => a.Stake == s).Select(a => (FolieAward?)a).FirstOrDefault();
			Player? winner = award is { } a ? bidders[a.Player] : null;
			if (winner == set.Player) continue;
			set.Rewards.Remove(reward);
			reward.OnSkipped();
			if (winner == null) continue;
			RewardsSet target = living.First(x => x.Player == winner);
			target.Rewards.Add(Rebuild(run, reward, winner));
			target.Rewards.Sort((x, y) => x.RewardsSetIndex.CompareTo(y.RewardsSetIndex));
		}
	}

	/// <summary>The same cards as a reward for <paramref name="winner"/>; a mandatory reward stays mandatory.</summary>
	public static CardReward Rebuild(IRunState run, CardReward reward, Player winner)
	{
		var cards = reward.Cards.ToList();
		foreach (CardModel card in cards)
		{
			run.RemoveCard(card);
			run.AddCard(card, winner);
		}
		var rebuilt = new CardReward(cards, CardCreationSource.Encounter, winner, RerollOptions(reward));
		if (!reward.CanSkip) MatriochkaRewards.MakeMandatory(rebuilt);
		return rebuilt;
	}

	/// <summary>Vanilla <c>CombatRoom.OfferRoomEndRewards</c> with the chest between generating and offering.</summary>
	public static async Task OfferRoomEnd(CombatRoom room)
	{
		var sets = new List<RewardsSet>();
		foreach (Player player in room.CombatState.Players)
			sets.Add(await RewardsCmd.GenerateForRoomEnd(player, room));
		IRunState run = room.CombatState.Players[0].RunState;
		await RedistributeSafely(run, $"combat_{run.TotalFloor}", sets);
		foreach (RewardsSet set in sets)
			_ = TaskHelper.RunSafely(set.Offer());
	}

	/// <summary>Vanilla <c>TreasureRoom.DoExtraRewardsIfNeeded</c> with the chest between generating and offering.</summary>
	public static async Task OfferTreasureExtras(IRunState run, TreasureRoom room)
	{
		var sets = new List<RewardsSet>();
		foreach (Player player in run.Players)
			sets.Add(await RewardsCmd.GenerateForRoomEnd(player, room));
		await RedistributeSafely(run, $"treasure_{run.TotalFloor}", sets);
		Task? local = null;
		foreach (RewardsSet set in sets)
		{
			Task task = TaskHelper.RunSafely(set.Offer());
			if (LocalContext.IsMe(set.Player)) local = task;
		}
		if (local != null) await local;
	}

	/// <summary>Matriochka outside combat: Dolly's Mirror, then the floor's mandatory rewards go through one chest.</summary>
	public static async Task OfferFloors(IRunState run, bool mirror, bool cards)
	{
		if (mirror)
		{
			foreach (Player player in run.Players.Where(p => !p.Creature.IsDead))
				await RelicCmd.Obtain(ModelDb.Relic<MegaCrit.Sts2.Core.Models.Relics.DollysMirror>().ToMutable(), player);
		}
		if (!cards) return;
		var sets = new List<RewardsSet>();
		foreach (Player player in run.Players.Where(p => !p.Creature.IsDead))
		{
			var options = CardCreationOptions.ForNonCombatWithDefaultOdds([player.Character.CardPool]);
			var set = new RewardsSet(player).WithCustomRewards([new CardReward(options, 3, player) { CanSkip = false }])
				.WithSkippingDisallowed();
			await set.GenerateWithoutOffering();
			sets.Add(set);
		}
		await RedistributeSafely(run, $"floor_{run.TotalFloor}", sets);
		foreach (RewardsSet set in sets)
			_ = TaskHelper.RunSafely(set.Offer());
	}

	private static async Task RedistributeSafely(IRunState run, string key, IReadOnlyList<RewardsSet> sets)
	{
		try
		{
			await Redistribute(run, key, sets);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: chest failed, rewards stay with their owners: {e}");
		}
	}
}

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OfferRoomEndRewards))]
public static class FolieCombatRewardsPatch
{
	public static bool Prefix(CombatRoom __instance, ref Task __result)
	{
		if (__instance.CombatState.Players.FirstOrDefault()?.RunState is not { } run || !SpoonFolieADeux.IsActive(run)) return true;
		__result = FolieRewards.OfferRoomEnd(__instance);
		return false;
	}
}

[HarmonyPatch(typeof(TreasureRoom), nameof(TreasureRoom.DoExtraRewardsIfNeeded))]
public static class FolieTreasureRewardsPatch
{
	private static readonly AccessTools.FieldRef<TreasureRoom, IRunState?> RunState =
		AccessTools.FieldRefAccess<TreasureRoom, IRunState?>("_runState");

	public static bool Prefix(TreasureRoom __instance, ref Task __result)
	{
		if (RunState(__instance) is not { } run || !SpoonFolieADeux.IsActive(run)) return true;
		__result = FolieRewards.OfferTreasureExtras(run, __instance);
		return false;
	}
}
