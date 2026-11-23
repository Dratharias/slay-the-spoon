using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ForgeLadderRuleTests
{
	[Fact]
	public void ShowsOnlyWhenTickedInForgeBelowLimit()
	{
		Assert.True(ForgeLadderRule.Shows(true, true, 9, 0));
		Assert.True(ForgeLadderRule.Shows(true, true, 9, 8));
		Assert.False(ForgeLadderRule.Shows(true, true, 9, 9));
		Assert.False(ForgeLadderRule.Shows(false, true, 9, 0));
		Assert.False(ForgeLadderRule.Shows(true, false, 9, 0));
		Assert.False(ForgeLadderRule.Shows(true, true, 1, 0));
	}

	[Fact]
	public void LevelsFromNextToLimit()
	{
		Assert.Equal(Enumerable.Range(1, 9), ForgeLadderRule.Levels(0, 9));
		Assert.Equal([5, 6, 7, 8, 9], ForgeLadderRule.Levels(4, 9));
		Assert.Empty(ForgeLadderRule.Levels(9, 9));
		Assert.Equal(Enumerable.Range(1, 9), ForgeLadderRule.Levels(-1, 9));
	}

	[Fact]
	public void CellsFillRowsOfThree()
	{
		Assert.Equal((0, 0), ForgeLadderRule.Cell(0));
		Assert.Equal((2, 0), ForgeLadderRule.Cell(2));
		Assert.Equal((0, 1), ForgeLadderRule.Cell(3));
		Assert.Equal((2, 2), ForgeLadderRule.Cell(8));
	}
}
