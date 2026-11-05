namespace SlaySpoon.Core;

/// <summary>How a player came out of a boss relic chest.</summary>
public enum BossChestOutcome
{
	/// <summary>The only voter for their relic.</summary>
	Only,
	/// <summary>Won the rock-paper-scissors for their relic.</summary>
	WonFight,
	/// <summary>Lost a fight and got a relic nobody voted for.</summary>
	Consolation,
	/// <summary>Lost a fight and no relic was left.</summary>
	Empty,
	/// <summary>Closed the chest without voting.</summary>
	Skipped,
}

/// <summary>One taker's result: the relic index is null for <see cref="BossChestOutcome.Empty"/> and Skipped.</summary>
public readonly record struct BossChestAward(int Player, int? Relic, BossChestOutcome Outcome);

/// <summary>
/// StS1 boss relics (Custom Run): the act 2 and act 3 Ancients offer a chest of <see cref="Size"/> boss relics. In co-op
/// the players who take the option share the chest and it resolves like StS2's treasure chest: a relic with one voter
/// goes to them, several voters fight (rock-paper-scissors) for it, and the losers get the relics nobody voted for, in a
/// shuffled order, while they last. A player who closes the chest gets nothing and no consolation.
/// </summary>
public static class BossChestRule
{
	public const int Size = 3;

	/// <summary>The Ancients that offer the chest: act 2 and act 3 (act indexes 1 and 2), never Neow.</summary>
	public static bool Offers(int actIndex, bool isNeow) => !isNeow && actIndex is 1 or 2;

	/// <summary>
	/// Darv's own rules for its native boss relics: Pandora's Box needs a run with no deck-clearing modifier; Ectoplasm
	/// and Sozu are act 2 only; Philosopher's Stone and Velvet Choker act 3 only.
	/// </summary>
	public static bool NativeAllowed(string relicEntry, int actIndex, bool clearsDeck) => relicEntry switch
	{
		"PANDORAS_BOX" => !clearsDeck,
		"ECTOPLASM" or "SOZU" => actIndex == 1,
		"PHILOSOPHERS_STONE" or "VELVET_CHOKER" => actIndex == 2,
		_ => true,
	};

	/// <summary>
	/// Character boss relics enter a shared chest only when every taker plays that character (a relic made for another
	/// character's mechanics would be dead weight; builder call). Solo, the player's own character always qualifies.
	/// </summary>
	public static bool CharacterRelicsShared<T>(IReadOnlyCollection<T> takerCharacters, T character) =>
		takerCharacters.Count > 0 && takerCharacters.All(c => EqualityComparer<T>.Default.Equals(c, character));

	/// <summary>
	/// Resolves the chest. <paramref name="votes"/> maps each taker to a relic index or null (closed the chest);
	/// <paramref name="fight"/> gets the voters (in player order) and the relic, and returns the winner; it is only called
	/// when several players voted for the same relic. <paramref name="shuffle"/> orders the relics nobody voted for.
	/// </summary>
	public static List<BossChestAward> Resolve(IReadOnlyDictionary<int, int?> votes, int relics,
		Func<IReadOnlyList<int>, int, int> fight, Func<List<int>, List<int>> shuffle)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(relics);
		foreach (var (player, vote) in votes)
			if (vote is { } v && (v < 0 || v >= relics))
				throw new ArgumentException($"Player {player} voted for relic {v} of {relics}.", nameof(votes));

		var awards = new List<BossChestAward>();
		var losers = new List<int>();
		var unvoted = new List<int>();
		for (int relic = 0; relic < relics; relic++)
		{
			var voters = votes.Where(v => v.Value == relic).Select(v => v.Key).OrderBy(p => p).ToList();
			if (voters.Count == 0) { unvoted.Add(relic); continue; }
			if (voters.Count == 1) { awards.Add(new(voters[0], relic, BossChestOutcome.Only)); continue; }
			int winner = fight(voters, relic);
			if (!voters.Contains(winner)) throw new InvalidOperationException($"Fight winner {winner} did not vote for relic {relic}.");
			awards.Add(new(winner, relic, BossChestOutcome.WonFight));
			losers.AddRange(voters.Where(p => p != winner));
		}
		losers.Sort();
		var left = shuffle(unvoted);
		for (int i = 0; i < losers.Count; i++)
			awards.Add(i < left.Count
				? new(losers[i], left[i], BossChestOutcome.Consolation)
				: new(losers[i], null, BossChestOutcome.Empty));
		foreach (var (player, vote) in votes.OrderBy(v => v.Key))
			if (vote == null) awards.Add(new(player, null, BossChestOutcome.Skipped));
		return awards.OrderBy(a => a.Player).ToList();
	}
}
