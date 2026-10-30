using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class HeartInvincibleRuleTests
{
	[Theory]
	[InlineData(0, 1, 300)] [InlineData(9, 1, 300)] [InlineData(10, 1, 200)]
	[InlineData(0, 2, 780)] [InlineData(10, 2, 520)] [InlineData(10, 3, 780)]
	public void SharedCapUsesApprovedBossScaling(int ascension, int players, int expected) =>
		Assert.Equal(expected, HeartInvincibleRule.InitialCap(ascension, players));

	[Theory]
	[InlineData(20, 30, false, 50)] [InlineData(20, 30, true, 20)]
	[InlineData(0, 30, false, 30)] [InlineData(0, 30, true, 0)]
	public void CapIsAfterBlockIncludingPoison(int left, int block, bool unblockable, int expected) =>
		Assert.Equal(expected, HeartInvincibleRule.DamageCap(left, block, unblockable));

	[Fact]
	public void EveryPlayersHitsShareTheBudget()
	{
		int remaining = HeartInvincibleRule.InitialCap(10, 2);
		remaining = HeartInvincibleRule.AfterHpLoss(remaining, 300);
		Assert.Equal(220, remaining);
		remaining = HeartInvincibleRule.AfterHpLoss(remaining, 200);
		Assert.Equal(20, HeartInvincibleRule.DamageCap(remaining, 0, false));
		Assert.Equal(0, HeartInvincibleRule.AfterHpLoss(remaining, 20));
	}

	[Fact] public void PoisonTicksDrainTheSameBudget()
	{
		int left = HeartInvincibleRule.AfterHpLoss(300, 280);
		Assert.Equal(20, HeartInvincibleRule.DamageCap(left, block: 50, unblockable: true));
		Assert.Equal(0, HeartInvincibleRule.AfterHpLoss(left, 20));
	}

	[Fact] public void SoloCapIgnoresTheMultiplayerFactor() => Assert.Equal(300, HeartInvincibleRule.InitialCap(0, 1));

	[Theory] [InlineData(30, 0, 30)] [InlineData(30, 40, 0)]
	public void BlockedHitsAndExhaustedBudget(int left, int lost, int expected) =>
		Assert.Equal(expected, HeartInvincibleRule.AfterHpLoss(left, lost));
}
