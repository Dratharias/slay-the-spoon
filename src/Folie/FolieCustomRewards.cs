using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Folie;

/// <summary>
/// Folie à deux on one player's custom rewards (events, relics, potions that offer a card reward): every client runs
/// the offer for that player, so the set is generated, its first card reward goes in a one-head chest every living
/// player bids on, and a winner other than the owner receives it in a reward screen of their own.
/// </summary>
public static class FolieCustomRewards
{
	private static readonly AccessTools.FieldRef<RewardsSet, bool> IsGenerated =
		AccessTools.FieldRefAccess<RewardsSet, bool>("_isGenerated");

	private static int _floor = -1;
	private static readonly Dictionary<(ulong, string), int> Counts = new();

	public static async Task Offer(Player player, List<Reward> rewards)
	{
		IRunState run = player.RunState;
		var set = new RewardsSet(player).WithCustomRewards(rewards);
		await set.GenerateWithoutOffering();
		var living = run.Players.Where(p => !p.Creature.IsDead).ToList();
		CardReward? stake = set.Rewards.OfType<CardReward>().FirstOrDefault(r => r.IsPopulated && r.Cards.Any());
		if (stake == null || living.Count < 2 || !living.Contains(player))
		{
			await set.Offer();
			return;
		}

		Player? winner = player;
		try
		{
			FolieOutcome outcome = await FolieChest.Run(run, NextKey(run, player, "custom"), living,
				[new FolieStake(player, stake.Cards.ToList())], consolation: false);
			winner = outcome.Awards.Select(a => (FolieAward?)a).FirstOrDefault() is { } a ? living[a.Player] : null;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: custom reward chest failed, the reward stays with its owner: {e}");
		}
		if (winner == player)
		{
			await set.Offer();
			return;
		}
		set.Rewards.Remove(stake);
		stake.OnSkipped();
		Task own = set.Rewards.Count > 0 ? set.Offer() : Task.CompletedTask;
		if (winner != null)
		{
			var won = new RewardsSet(winner).WithCustomRewards([FolieRewards.Rebuild(run, stake, winner)]);
			IsGenerated(won) = true;
			_ = TaskHelper.RunSafely(won.Offer());
		}
		await own;
	}

	/// <summary>The owner's slot, the floor and the rank of this offer for that owner on the floor: the same on every client.</summary>
	internal static string NextKey(IRunState run, Player player, string kind)
	{
		if (_floor != run.TotalFloor)
		{
			_floor = run.TotalFloor;
			Counts.Clear();
		}
		int n = Counts.GetValueOrDefault((player.NetId, kind));
		Counts[(player.NetId, kind)] = n + 1;
		return $"{kind}_{player.NetId}_{run.TotalFloor}_{n}";
	}
}

[HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferCustom))]
public static class FolieCustomRewardsPatch
{
	public static bool Prefix(Player player, List<Reward> rewards, ref Task __result)
	{
		if (!SpoonFolieADeux.IsActive(player.RunState) || !rewards.OfType<CardReward>().Any()) return true;
		__result = FolieCustomRewards.Offer(player, rewards);
		return false;
	}
}
