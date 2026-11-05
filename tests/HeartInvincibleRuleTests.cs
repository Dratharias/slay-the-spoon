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

	/// <summary>A blockable hit may exceed the budget by the Block it meets; only the HP it removes drains the budget.</summary>
	[Fact] public void BlockedHitsDrainOnlyTheirHpLoss()
	{
		Assert.Equal(70, HeartInvincibleRule.DamageCap(20, block: 50, unblockable: false));
		// The 70 hit: 50 into Block, 20 into HP.
		Assert.Equal(0, HeartInvincibleRule.AfterHpLoss(20, 20));
		// A hit fully absorbed by Block leaves the budget whole.
		Assert.Equal(20, HeartInvincibleRule.AfterHpLoss(20, 0));
	}

	/// <summary>The cap depends only on ascension and players: the signature carries no Monster HP percent.</summary>
	[Fact] public void InvincibleBudgetIgnoresMonsterHpPercent()
	{
		Assert.Equal(780, HeartInvincibleRule.InitialCap(9, 2));
		Assert.Equal(520, HeartInvincibleRule.InitialCap(10, 2));
		Assert.Equal(2, typeof(HeartInvincibleRule).GetMethod(nameof(HeartInvincibleRule.InitialCap))!.GetParameters().Length);
	}

	[Theory] [InlineData(10, 30, 10)] [InlineData(30, 30, 30)] [InlineData(45, 30, 30)] [InlineData(45, 0, 0)] [InlineData(45, -5, 0)]
	public void HpLossIsClampedToTheBudgetLeft(int amount, int remaining, int expected) =>
		Assert.Equal(expected, HeartInvincibleRule.ClampHpLoss(amount, remaining));

	[Fact] public void SoloCapIgnoresTheMultiplayerFactor() => Assert.Equal(300, HeartInvincibleRule.InitialCap(0, 1));

	[Theory] [InlineData(30, 0, 30)] [InlineData(30, -5, 30)] [InlineData(30, 40, 0)] [InlineData(0, 10, 0)]
	public void BudgetNeverGrowsNorGoesNegative(int left, int lost, int expected) =>
		Assert.Equal(expected, HeartInvincibleRule.AfterHpLoss(left, lost));
}
