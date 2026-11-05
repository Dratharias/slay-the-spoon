namespace SlaySpoon.Core;

/// <summary>A relic handed out by a rematch round: <see cref="Fought"/> when several players voted for it.</summary>
public readonly record struct RematchAward(int Player, int Relic, bool Fought);

/// <summary>What one round decided.</summary>
public sealed class RematchRound
{
	public List<RematchAward> Awards { get; } = [];
	/// <summary>The player who took the Sapphire Key this round.</summary>
	public int? KeyTakenBy { get; internal set; }
	/// <summary>True when several players voted for the key and fought for it.</summary>
	public bool KeyFought { get; internal set; }
	/// <summary>The relic the last player got without a choice (no key left to choose).</summary>
	public RematchAward? Consolation { get; internal set; }
	/// <summary>True when the chest is done: <see cref="ChestRematchRule.Remaining"/> then lists the relics nobody took.</summary>
	public bool Finished { get; internal set; }
}

/// <summary>
/// Shared chest picking with rematches, for runs where the Sapphire Key is still to collect. Players vote for a relic;
/// a relic with one voter goes to them, several voters fight (rock-paper-scissors) and the losers vote again among the
/// relics left, instead of the vanilla random consolation prize. The Sapphire Key is one more choice from the first
/// round: whoever takes it gets no relic, and several key voters fight for it like for a relic. Players and relics are
/// indexes; the key's vote index is <see cref="KeyIndex"/>.
/// </summary>
public sealed class ChestRematchRule
{
	private readonly List<int> _active;
	private readonly List<int> _remaining;
	private bool _keyAvailable;

	public ChestRematchRule(int players, int relics, bool keyAvailable)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(players);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(relics);
		_active = Enumerable.Range(0, players).ToList();
		_remaining = Enumerable.Range(0, relics).ToList();
		_keyAvailable = keyAvailable;
		KeyIndex = relics;
	}

	public int KeyIndex { get; }
	public IReadOnlyList<int> Active => _active;
	public IReadOnlyList<int> Remaining => _remaining;
	public int Round { get; private set; } = 1;
	public bool Finished { get; private set; }

	/// <summary>The key is still in the chest (nobody took it yet).</summary>
	public bool KeyAvailable => _keyAvailable;

	/// <summary>The key is on offer to every active player while it is in the chest and a relic is left to give up.</summary>
	public bool KeyOffered => !Finished && _keyAvailable && _remaining.Count > 0;

	public bool IsActive(int player) => !Finished && _active.Contains(player);

	/// <summary>What <paramref name="player"/> may vote for this round; null (leave with nothing) is always allowed.</summary>
	public bool CanVote(int player, int? index) =>
		IsActive(player) && (index is not { } i || _remaining.Contains(i) || (i == KeyIndex && KeyOffered));

	/// <summary>Every vote index on offer this round, the key last.</summary>
	public List<int> Choices() => KeyOffered ? [.. _remaining, KeyIndex] : [.. _remaining];

	/// <summary>
	/// Resolves a round once every active player voted. <paramref name="fight"/> gets the voters (in player order) and the
	/// relic (or <see cref="KeyIndex"/>), and returns the winner; it is only called when several players voted for the
	/// same choice.
	/// </summary>
	public RematchRound Resolve(IReadOnlyDictionary<int, int?> votes, Func<IReadOnlyList<int>, int, int> fight)
	{
		if (Finished) throw new InvalidOperationException("The chest is already done.");
		foreach (int player in _active)
			if (!votes.TryGetValue(player, out var vote) || !CanVote(player, vote))
				throw new ArgumentException($"Player {player} has no valid vote.", nameof(votes));

		var round = new RematchRound();
		var leaving = new List<int>();
		leaving.AddRange(_active.Where(p => votes[p] == null));
		var keyVoters = _active.Where(p => votes[p] == KeyIndex).ToList();
		if (keyVoters.Count > 0)
		{
			int taker = keyVoters.Count == 1 ? keyVoters[0] : fight(keyVoters, KeyIndex);
			if (!keyVoters.Contains(taker)) throw new InvalidOperationException($"Fight winner {taker} did not vote for the key.");
			round.KeyTakenBy = taker;
			round.KeyFought = keyVoters.Count > 1;
			leaving.Add(taker);
		}
		foreach (int relic in _remaining.ToList())
		{
			var voters = _active.Where(p => votes[p] == relic).ToList();
			if (voters.Count == 0) continue;
			int winner = voters.Count == 1 ? voters[0] : fight(voters, relic);
			if (!voters.Contains(winner)) throw new InvalidOperationException($"Fight winner {winner} did not vote for {relic}.");
			round.Awards.Add(new RematchAward(winner, relic, voters.Count > 1));
			leaving.Add(winner);
			_remaining.Remove(relic);
		}
		_active.RemoveAll(leaving.Contains);
		if (round.KeyTakenBy != null) _keyAvailable = false;

		// The last player with one relic left and no key to choose gets it, as in vanilla.
		if (_active.Count == 1 && _remaining.Count == 1 && !_keyAvailable)
		{
			round.Consolation = new RematchAward(_active[0], _remaining[0], Fought: false);
			_active.Clear();
			_remaining.Clear();
		}
		Finished = _active.Count == 0 || _remaining.Count == 0;
		round.Finished = Finished;
		if (!Finished) Round++;
		return round;
	}
}
