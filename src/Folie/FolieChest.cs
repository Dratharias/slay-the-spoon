using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Folie;

/// <summary>What one head of a Folie à deux chest holds: its owner (the head shown) and the cards at stake.</summary>
public sealed record FolieStake(Player Owner, IReadOnlyList<CardModel> Cards);

/// <summary>A resolved chest: who took which stake, and the rock-paper-scissors fight behind each fought stake.</summary>
public sealed record FolieOutcome(List<FolieAward> Awards, Dictionary<int, RelicPickingFight> Fights);

/// <summary>
/// The Folie à deux chest (docs/superpowers/specs/2026-11-14-folie-a-deux-design.md): every bidder locks one bid,
/// sent as a <see cref="FolieBidMessage"/> under the chest's key; the stakes are then resolved by <see cref="FolieRule"/>
/// with fights drawn on a stream seeded from the run seed and the key, the same on every client.
/// </summary>
public static class FolieChest
{
	/// <param name="key">Names the chest, the same on every client (e.g. <c>combat_12</c>); it keys the bids and the fights.</param>
	/// <param name="bidders">Living players taking part, in slot order.</param>
	/// <param name="consolation">Losers take the unclaimed stakes (deck-clearing sources).</param>
	public static async Task<FolieOutcome> Run(IRunState run, string key, IReadOnlyList<Player> bidders, IReadOnlyList<FolieStake> stakes,
		bool consolation)
	{
		var rng = new Rng(run.Rng.Seed, "SPOON_FOLIE_" + key);
		uint chest = FolieBids.Hash(key);
		var view = FolieChestOverlay.Show(run, bidders, stakes);
		var bids = new int?[bidders.Count];
		var tasks = bidders.Select(async (player, i) =>
		{
			int? bid;
			if (LocalContext.IsMe(player))
			{
				bid = view != null ? await view.PickLocal() : stakes.Count > 0 ? 0 : null;
				FolieBids.SendLocal(chest, player, bid);
			}
			else
			{
				bid = await FolieBids.WaitFor(chest, player);
			}
			if (bid is < 0 || bid >= stakes.Count) bid = null;
			bids[i] = bid;
			view?.ShowBid(i, bid);
		}).ToList();
		await Task.WhenAll(tasks);
		FolieBids.Forget(chest, bidders);

		var moves = Enum.GetValues<RelicPickingFightMove>();
		var fights = new Dictionary<int, RelicPickingFight>();
		List<FolieAward> awards = FolieRule.Resolve(bids, stakes.Count, consolation,
			fighters =>
			{
				int stake = bids[fighters[0]]!.Value;
				RelicPickingResult result = RelicPickingResult.GenerateRelicFight(fighters.Select(f => bidders[f]).ToList(),
					null!, () => rng.NextItem(moves));
				fights[stake] = result.fight!;
				return bidders.ToList().IndexOf(result.player!);
			},
			left => rng.NextInt(left));
		SpoonLog.Info($"folie: {key}, bids [{string.Join(", ", bids.Select(b => b?.ToString() ?? "-"))}], "
			+ $"awards [{string.Join(", ", awards.Select(a => $"{a.Player}->{a.Stake}{(a.Consolation ? "c" : "")}"))}]");
		var outcome = new FolieOutcome(awards, fights);
		if (view != null) await view.PlayResults(outcome);
		return outcome;
	}
}
