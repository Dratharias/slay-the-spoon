using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class OvercursedRuleTests
{
	[Fact]
	public void CursesComeInThrees()
	{
		Assert.Equal(2, OvercursedRule.CopiesFor(true, false, 20));
		Assert.Equal(0, OvercursedRule.CopiesFor(false, false, 20));
	}

	[Fact]
	public void NoFiveOfAKindWithHoarderOrMatriochka() =>
		Assert.Equal(0, OvercursedRule.CopiesFor(true, true, 20));

	[Fact]
	public void StopsAtTheDeckLimit()
	{
		Assert.Equal(1, OvercursedRule.CopiesFor(true, false, MatriochkaRule.DeckLimit - 1));
		Assert.Equal(0, OvercursedRule.CopiesFor(true, false, MatriochkaRule.DeckLimit));
	}

	[Fact]
	public void EmbraceWithEitherMode()
	{
		Assert.True(OvercursedRule.EmbraceOffered(true, false));
		Assert.True(OvercursedRule.EmbraceOffered(false, true));
		Assert.False(OvercursedRule.EmbraceOffered(false, false));
	}

	[Fact]
	public void MidasAlwaysPaysGoldBloodHeals()
	{
		Assert.Equal(CurseEventReward.Gold, OvercursedRule.EmbraceReward(CurseEventReward.Heal, false, true));
		Assert.Equal(CurseEventReward.Gold, OvercursedRule.EmbraceReward(CurseEventReward.CursedRelic, false, true));
		Assert.Equal(CurseEventReward.Heal, OvercursedRule.EmbraceReward(CurseEventReward.Gold, true, false));
		Assert.Equal(CurseEventReward.Heal, OvercursedRule.EmbraceReward(CurseEventReward.Gold, true, true));
		Assert.Equal(CurseEventReward.CursedRelic, OvercursedRule.EmbraceReward(CurseEventReward.CursedRelic, false, false));
		Assert.Equal(150, OvercursedRule.EmbraceGold(true));
		Assert.Equal(75, OvercursedRule.EmbraceGold(false));
	}
}
