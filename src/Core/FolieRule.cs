namespace SlaySpoon.Core;

/// <summary>A stake handed out by a Folie à deux chest.</summary>
/// <param name="Player">Player index.</param>
/// <param name="Stake">Stake index.</param>
/// <param name="Fought">Several players bid on it and fought (rock-paper-scissors).</param>
/// <param name="Consolation">The player lost their bid and got an unclaimed stake (deck-clearing sources only).</param>
public readonly record struct FolieAward(int Player, int Stake, bool Fought, bool Consolation);

/// <summary>
/// Resolution of a Folie à deux chest (docs/superpowers/specs/2026-11-14-folie-a-deux-design.md): every player locks
/// one bid on a stake; a stake with one bidder goes to them, several bidders fight, a stake nobody bid on is lost.
/// With <c>consolation</c> (Draft, Sealed Deck), losers take the unclaimed stakes so every bidder ends with one.
/// </summary>
public static class FolieRule
{
	/// <param name="bids"><c>bids[player]</c>: the stake the player bid on, or null for a player out of the chest.</param>
	/// <param name="stakes">Number of stakes.</param>
	/// <param name="consolation">Hand unclaimed stakes to the losers.</param>
	/// <param name="fight">Returns the winner among several bidders (in player order).</param>
	/// <param name="pick">Returns an index below its argument: which unclaimed stake the next loser takes.</param>
	public static List<FolieAward> Resolve(IReadOnlyList<int?> bids, int stakes, bool consolation,
		Func<IReadOnlyList<int>, int> fight, Func<int, int> pick)
	{
		var awards = new List<FolieAward>();
		var winners = new HashSet<int>();
		var unclaimed = new List<int>();
		for (int stake = 0; stake < stakes; stake++)
		{
			var bidders = Enumerable.Range(0, bids.Count).Where(p => bids[p] == stake).ToList();
			if (bidders.Count == 0)
			{
				unclaimed.Add(stake);
				continue;
			}
			int winner = bidders.Count == 1 ? bidders[0] : fight(bidders);
			if (!bidders.Contains(winner))
				throw new InvalidOperationException($"Fight winner {winner} did not bid on stake {stake}.");
			winners.Add(winner);
			awards.Add(new FolieAward(winner, stake, bidders.Count > 1, false));
		}
		if (consolation)
		{
			foreach (int loser in Enumerable.Range(0, bids.Count).Where(p => bids[p] != null && !winners.Contains(p)))
			{
				if (unclaimed.Count == 0)
					break;
				int i = unclaimed.Count == 1 ? 0 : pick(unclaimed.Count);
				awards.Add(new FolieAward(loser, unclaimed[i], false, true));
				unclaimed.RemoveAt(i);
			}
		}
		awards.Sort((a, b) => a.Stake.CompareTo(b.Stake));
		return awards;
	}
}
