using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ChestRematchRuleTests
{
	private static Dictionary<int, int?> Votes(params (int Player, int? Vote)[] votes) =>
		votes.ToDictionary(v => v.Player, v => v.Vote);

	private static int FirstWins(IReadOnlyList<int> voters, int relic) => voters[0];

	[Fact]
	public void Solo_OffersTheKeyFromTheStart()
	{
		var rule = new ChestRematchRule(1, 1, keyAvailable: true);
		Assert.True(rule.KeyOffered);
		Assert.Equal([0, 1], rule.Choices());
		var round = rule.Resolve(Votes((0, rule.KeyIndex)), FirstWins);
		Assert.Equal(0, round.KeyTakenBy);
		Assert.Empty(round.Awards);
		Assert.True(round.Finished);
		Assert.Equal([0], rule.Remaining);
	}

	[Fact]
	public void Solo_RelicLeavesTheKey()
	{
		var rule = new ChestRematchRule(1, 1, keyAvailable: true);
		var round = rule.Resolve(Votes((0, 0)), FirstWins);
		Assert.Null(round.KeyTakenBy);
		Assert.Equal([new RematchAward(0, 0, false)], round.Awards);
		Assert.True(round.Finished);
	}

	[Fact]
	public void DistinctVotes_FinishInOneRoundWithoutTheKey()
	{
		var rule = new ChestRematchRule(2, 2, keyAvailable: true);
		var round = rule.Resolve(Votes((0, 1), (1, 0)), FirstWins);
		Assert.True(round.Finished);
		Assert.Null(round.KeyTakenBy);
		Assert.Equal(2, round.Awards.Count);
	}

	[Fact]
	public void TwoPlayers_TheLoserChoosesTheLastRelicOrTheKey()
	{
		var rule = new ChestRematchRule(2, 2, keyAvailable: true);
		var round = rule.Resolve(Votes((0, 0), (1, 0)), (voters, _) => voters[1]);
		Assert.Equal([new RematchAward(1, 0, true)], round.Awards);
		Assert.False(round.Finished);
		Assert.Equal(2, rule.Round);
		Assert.Equal([0], rule.Active);
		Assert.True(rule.KeyOffered);
		Assert.True(rule.CanVote(0, rule.KeyIndex));
		Assert.False(rule.CanVote(1, 1));
		Assert.False(rule.CanVote(0, 0));

		var last = rule.Resolve(Votes((0, rule.KeyIndex)), FirstWins);
		Assert.Equal(0, last.KeyTakenBy);
		Assert.True(last.Finished);
		Assert.Equal([1], rule.Remaining);
	}

	[Fact]
	public void NoKey_TheLastPlayerGetsTheLastRelic()
	{
		var rule = new ChestRematchRule(2, 2, keyAvailable: false);
		var round = rule.Resolve(Votes((0, 1), (1, 1)), FirstWins);
		Assert.True(round.Finished);
		Assert.Equal(new RematchAward(1, 0, false), round.Consolation);
		Assert.Empty(rule.Remaining);
	}

	[Fact]
	public void ThreePlayers_LosersRematchAmongTheRelicsLeft()
	{
		var rule = new ChestRematchRule(3, 3, keyAvailable: true);
		var first = rule.Resolve(Votes((0, 2), (1, 2), (2, 2)), (voters, _) => voters[2]);
		Assert.Equal([new RematchAward(2, 2, true)], first.Awards);
		Assert.Equal([0, 1], rule.Active);
		Assert.True(rule.KeyOffered);
		Assert.Equal([0, 1, 3], rule.Choices());

		var second = rule.Resolve(Votes((0, 0), (1, 0)), FirstWins);
		Assert.Equal([new RematchAward(0, 0, true)], second.Awards);
		Assert.Equal([1], rule.Active);
		Assert.True(rule.KeyOffered);
		Assert.Equal([1, 3], rule.Choices());

		var third = rule.Resolve(Votes((1, 1)), FirstWins);
		Assert.True(third.Finished);
		Assert.Equal([new RematchAward(1, 1, false)], third.Awards);
		Assert.Empty(rule.Remaining);
	}

	[Fact]
	public void LeavingWithNothing_KeepsTheRelicForTheOthers()
	{
		var rule = new ChestRematchRule(3, 3, keyAvailable: true);
		var round = rule.Resolve(Votes((0, null), (1, 0), (2, 0)), FirstWins);
		Assert.Equal([2], rule.Active);
		Assert.Equal([1, 2], rule.Remaining);
		Assert.True(rule.KeyOffered);
		Assert.False(round.Finished);
	}

	[Fact]
	public void Resolve_RefusesMissingOrInvalidVotes()
	{
		var rule = new ChestRematchRule(2, 2, keyAvailable: true);
		Assert.Throws<ArgumentException>(() => rule.Resolve(Votes((0, 0)), FirstWins));
		Assert.Throws<ArgumentException>(() => rule.Resolve(Votes((0, 0), (1, rule.KeyIndex + 1)), FirstWins));
	}

	[Fact]
	public void TheKey_IsVotableFromTheFirstRound()
	{
		var rule = new ChestRematchRule(3, 3, keyAvailable: true);
		Assert.True(rule.KeyOffered);
		Assert.True(rule.CanVote(1, rule.KeyIndex));
		var round = rule.Resolve(Votes((0, 0), (1, rule.KeyIndex), (2, 2)), FirstWins);
		Assert.Equal(1, round.KeyTakenBy);
		Assert.False(round.KeyFought);
		Assert.False(rule.KeyAvailable);
		Assert.True(round.Finished);
		Assert.Equal([1], rule.Remaining);
	}

	[Fact]
	public void SeveralKeyVoters_FightAndTheLosersVoteAgain()
	{
		var rule = new ChestRematchRule(3, 3, keyAvailable: true);
		var asked = new List<(int[], int)>();
		var round = rule.Resolve(Votes((0, rule.KeyIndex), (1, rule.KeyIndex), (2, 0)),
			(voters, choice) => { asked.Add((voters.ToArray(), choice)); return voters[1]; });
		var (fighters, fought) = Assert.Single(asked);
		Assert.Equal([0, 1], fighters);
		Assert.Equal(rule.KeyIndex, fought);
		Assert.Equal(1, round.KeyTakenBy);
		Assert.True(round.KeyFought);
		Assert.Equal([new RematchAward(2, 0, false)], round.Awards);
		Assert.Null(round.Consolation);
		Assert.False(round.Finished);
		Assert.Equal([0], rule.Active);
		Assert.Equal([1, 2], rule.Remaining);
		Assert.Equal([1, 2], rule.Choices());
	}

	[Fact]
	public void AKeyFightLoser_KeepsChoosingAmongTheRelicsLeft()
	{
		var rule = new ChestRematchRule(4, 4, keyAvailable: true);
		var round = rule.Resolve(Votes((0, rule.KeyIndex), (1, rule.KeyIndex), (2, 0), (3, 1)), (voters, _) => voters[0]);
		Assert.Equal(0, round.KeyTakenBy);
		Assert.False(round.Finished);
		Assert.Equal([1], rule.Active);
		Assert.Equal([2, 3], rule.Remaining);
		Assert.False(rule.KeyOffered);
		Assert.False(rule.CanVote(1, rule.KeyIndex));
		Assert.Equal([2, 3], rule.Choices());
	}

	[Fact]
	public void AKeyFightWinnerWhoIsNotAVoter_Throws()
	{
		var rule = new ChestRematchRule(2, 2, keyAvailable: true);
		Assert.Throws<InvalidOperationException>(() =>
			rule.Resolve(Votes((0, rule.KeyIndex), (1, rule.KeyIndex)), (_, _) => 5));
	}

	[Fact]
	public void Fight_IsOnlyAskedForSharedVotes()
	{
		var rule = new ChestRematchRule(3, 3, keyAvailable: false);
		var asked = new List<(int[], int)>();
		var round = rule.Resolve(Votes((0, 0), (1, 1), (2, 1)), (voters, relic) => { asked.Add((voters.ToArray(), relic)); return voters[1]; });
		var (fighters, fought) = Assert.Single(asked);
		Assert.Equal([1, 2], fighters);
		Assert.Equal(1, fought);
		Assert.Equal([new RematchAward(0, 0, false), new RematchAward(2, 1, true)], round.Awards);
		Assert.Equal(new RematchAward(1, 2, false), round.Consolation);
		Assert.True(round.Finished);
	}
}
