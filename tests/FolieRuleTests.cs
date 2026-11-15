using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class FolieRuleTests
{
	private static int LastWins(IReadOnlyList<int> bidders) => bidders[^1];
	private static int First(int count) => 0;

	[Fact]
	public void SingleBidder_TakesItUnfought()
	{
		var awards = FolieRule.Resolve([1, 0], 2, false, LastWins, First);
		Assert.Equal([new FolieAward(1, 0, false, false), new FolieAward(0, 1, false, false)], awards);
	}

	[Fact]
	public void SeveralBidders_FightAndLoserGetsNothing()
	{
		var awards = FolieRule.Resolve([0, 0, 0], 3, false, LastWins, First);
		Assert.Equal([new FolieAward(2, 0, true, false)], awards);
	}

	[Fact]
	public void UnbidStake_IsLost()
	{
		var awards = FolieRule.Resolve([2, 2], 3, false, LastWins, First);
		Assert.Single(awards);
		Assert.Equal(2, awards[0].Stake);
	}

	[Fact]
	public void Consolation_GivesEveryBidderOneStake()
	{
		var awards = FolieRule.Resolve([0, 0, 0, 1], 4, true, LastWins, n => n - 1);
		Assert.Equal(4, awards.Count);
		Assert.Equal([0, 1, 2, 3], awards.Select(a => a.Player).Order());
		Assert.Equal([0, 1, 2, 3], awards.Select(a => a.Stake));
		Assert.Equal(new FolieAward(2, 0, true, false), awards[0]);
		Assert.Equal(new FolieAward(3, 1, false, false), awards[1]);
		Assert.Equal(new FolieAward(1, 2, false, true), awards[2]);
		Assert.Equal(new FolieAward(0, 3, false, true), awards[3]);
	}

	[Fact]
	public void PlayerOutOfTheChest_GetsNothing()
	{
		var awards = FolieRule.Resolve([0, null], 1, true, LastWins, First);
		Assert.Equal([new FolieAward(0, 0, false, false)], awards);
	}
}
