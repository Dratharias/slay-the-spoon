using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ConductorRuleTests
{
	[Fact]
	public void FirstConductorCanBeAnyone()
	{
		Assert.Equal(3, ConductorRule.Rolls(3, -1));
		Assert.Equal(new[] { 0, 1, 2 }, Enumerable.Range(0, 3).Select(r => ConductorRule.Pick(3, -1, r)));
	}

	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	public void NeverTheSameConductorTwiceInARow(int players)
	{
		for (int previous = 0; previous < players; previous++)
		{
			int rolls = ConductorRule.Rolls(players, previous);
			Assert.Equal(players - 1, rolls);
			var picks = Enumerable.Range(0, rolls).Select(r => ConductorRule.Pick(players, previous, r)).ToHashSet();
			Assert.DoesNotContain(previous, picks);
			Assert.Equal(players - 1, picks.Count);
		}
	}

	[Fact]
	public void SoloIsAlwaysSlotZero() => Assert.Equal(0, ConductorRule.Pick(1, 0, 0));

	[Fact]
	public void RecipientsFollowThePicks() =>
		Assert.Equal(new[] { 1, 0, 1 }, ConductorRule.Recipients(new[] { 1, 0, 1 }, new[] { 0, 0, 1 }, 2));

	[Fact]
	public void BadPicksLeaveTheRewardWithItsOwner()
	{
		Assert.Equal(new[] { 0, 1, 2 }, ConductorRule.Recipients(null, new[] { 0, 1, 2 }, 3));
		Assert.Equal(new[] { 2, 1, 2 }, ConductorRule.Recipients(new[] { 2 }, new[] { 0, 1, 2 }, 3));
		Assert.Equal(new[] { 0, 1 }, ConductorRule.Recipients(new[] { -1, 51 }, new[] { 0, 1 }, 3));
	}

	[Theory]
	[InlineData(0, -1, 2)]
	[InlineData(1, 0, 2)]
	[InlineData(3, 2, 4)]
	[InlineData(2, 15, 3)]
	public void PicksRoundTrip(int slot, int card, int players) =>
		Assert.Equal((slot, card), ConductorRule.Decode(ConductorRule.Encode(slot, card, players), players));

	[Fact]
	public void ClickedCardsGoWithTheirReward()
	{
		int[] picks = { ConductorRule.Encode(1, 2, 2), ConductorRule.Encode(0, -1, 2), ConductorRule.Encode(1, -1, 2) };
		Assert.Equal(new[] { 1, 0, 1 }, ConductorRule.Recipients(picks, new[] { 0, 0, 1 }, 2));
		Assert.Equal(new[] { 2, -1, -1 }, ConductorRule.CardPicks(picks, new[] { 3, 3, 0 }, 2));
	}

	[Fact]
	public void BadCardPicksAreDropped()
	{
		Assert.Equal(new[] { -1, -1 }, ConductorRule.CardPicks(null, new[] { 3, 3 }, 2));
		Assert.Equal(new[] { -1, -1 }, ConductorRule.CardPicks(new[] { ConductorRule.Encode(0, 3, 2), 99 }, new[] { 3, 3 }, 2));
	}

	[Fact]
	public void TwoPlayersShareOneLineOfPortraitsThreeOrMoreASquare()
	{
		Assert.Equal(2, ConductorRule.HeadColumns(2));
		Assert.Equal(2, ConductorRule.HeadColumns(3));
		Assert.Equal(2, ConductorRule.HeadColumns(4));
	}

	[Theory]
	[InlineData(2, 1920, 1080, 1)]   // two rows fit in one column
	[InlineData(6, 1920, 1080, 2)]   // six rows are too tall for one column
	[InlineData(12, 3440, 1440, 3)]  // ultrawide: three columns shrink least
	[InlineData(8, 1920, 1080, 2)]   // 16:9: a third column costs more width than it saves
	[InlineData(12, 1280, 800, 3)]   // Steam Deck: everything shrinks, three columns least
	public void ColumnsShrinkThePageLeast(int rewards, float viewW, float viewH, int columns) =>
		Assert.Equal(columns, ConductorRule.Columns(rewards, 800, 250, 150, 60, 14, viewW, viewH));
}
