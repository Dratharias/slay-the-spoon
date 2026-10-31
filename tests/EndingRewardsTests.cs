using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingRewardsTests
{
	// isBoss, onLastAct, nextActIsEnding, hasSecondBoss, atFirstBoss
	[Fact]
	public void FirstA10Act3BossBeforeTheEndingGivesNoRewards() =>
		Assert.True(EndingBossRewardRule.SuppressRewards(true, false, true, true, true));

	[Fact]
	public void SecondBossKeepsItsRewards() =>
		Assert.False(EndingBossRewardRule.SuppressRewards(true, false, true, true, false));

	[Fact]
	public void SingleBossBelowA10KeepsItsRewards() =>
		Assert.False(EndingBossRewardRule.SuppressRewards(true, false, true, false, true));

	[Fact]
	public void EarlierActsAndNonBossRoomsAreUntouched()
	{
		Assert.False(EndingBossRewardRule.SuppressRewards(true, false, false, true, true));
		Assert.False(EndingBossRewardRule.SuppressRewards(false, false, true, true, true));
	}

	[Fact]
	public void LastActIsLeftToVanilla() =>
		Assert.False(EndingBossRewardRule.SuppressRewards(true, true, true, true, true));

	[Fact]
	public void SmashBlockIsTheHighestPreBlockHit()
	{
		Assert.Equal(38, EndingNumbers.ShieldSmashBlock(9, [38, 12]));
		Assert.Equal(34, EndingNumbers.ShieldSmashBlock(8, [34]));
	}

	[Fact]
	public void SmashBlockCountsBlockedAndOverkillDamage()
	{
		// A hit of 38 fully absorbed by block, and one that overkills: both read as 38.
		Assert.Equal(38, EndingNumbers.ShieldSmashBlock(9, [38, 38]));
	}

	[Fact]
	public void SmashBlockIsFlat99AtA10AndZeroWithoutHits()
	{
		Assert.Equal(99, EndingNumbers.ShieldSmashBlock(10, [38]));
		Assert.Equal(0, EndingNumbers.ShieldSmashBlock(8, []));
	}

	[Theory]
	[InlineData(true, 123456L, true)]
	[InlineData(true, 0L, false)]
	[InlineData(false, 123456L, false)]
	public void WinTimeIsResetOnlyWhenTheEndingWasJustAppended(bool appended, long winTime, bool reset) =>
		Assert.Equal(reset, EndingRouteRule.ShouldResetWinTime(appended, winTime));

	[Fact]
	public void StingerVolumeFollowsMasterAndMusicSquared()
	{
		Assert.Equal(0.0625f, StingerVolume.Linear(0.5f, 0.5f), 4);
		Assert.Equal(1f, StingerVolume.Linear(1f, 1f), 4);
		Assert.Equal(0f, StingerVolume.Linear(0f, 1f), 4);
		Assert.Equal(1f, StingerVolume.Linear(2f, 2f), 4);
	}
}
