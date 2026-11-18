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
}
