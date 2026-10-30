using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherSkillMathTests
{
	[Theory]
	[InlineData(0, 10, 10)]
	[InlineData(4, 10, 6)]
	[InlineData(10, 10, 0)]
	[InlineData(12, 10, 0)]
	public void CardsToFillHand_NeverNegative(int hand, int max, int expected)
	{
		Assert.Equal(expected, WatcherSkillMath.CardsToFillHand(hand, max));
	}

	[Theory]
	[InlineData(0, 2, 0)]
	[InlineData(5, 2, 10)]
	[InlineData(5, 3, 15)]
	[InlineData(-1, 2, 0)]
	public void SpiritShieldBlock_IsPerCardTimesOthers(int cards, int perCard, int expected)
	{
		Assert.Equal(expected, WatcherSkillMath.SpiritShieldBlock(cards, perCard));
	}

	[Theory]
	[InlineData(30, 30, true)]
	[InlineData(31, 30, false)]
	[InlineData(1, 30, true)]
	[InlineData(0, 30, true)]
	public void JudgementKills_AtOrBelowCutoff(int hp, int cutoff, bool expected)
	{
		Assert.Equal(expected, WatcherSkillMath.JudgementKills(hp, cutoff));
	}

	[Theory]
	[InlineData(0, ForeignRarity.Common)]
	[InlineData(54, ForeignRarity.Common)]
	[InlineData(55, ForeignRarity.Uncommon)]
	[InlineData(84, ForeignRarity.Uncommon)]
	[InlineData(85, ForeignRarity.Rare)]
	[InlineData(99, ForeignRarity.Rare)]
	public void ForeignInfluence_RarityFromRoll_Is55_30_15(int roll, ForeignRarity expected)
	{
		Assert.Equal(expected, WatcherSkillMath.RarityFromRoll(roll));
	}

	[Theory]
	[InlineData(3, false, 3)]
	[InlineData(3, true, 4)]
	[InlineData(0, true, 1)]
	[InlineData(0, false, 0)]
	[InlineData(-2, false, 0)]
	public void CollectTurns_AddsUpgradeAndClamps(int x, bool upgraded, int expected)
	{
		Assert.Equal(expected, WatcherSkillMath.CollectTurns(x, upgraded));
	}
}
