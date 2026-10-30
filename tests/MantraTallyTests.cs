using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MantraTallyTests
{
	[Fact]
	public void StartsAtZero() => Assert.Equal(0, new MantraTally().Total);

	[Fact]
	public void AddsEveryGainEvenAfterDivinity()
	{
		var tally = new MantraTally();
		tally.Add(7);
		tally.Add(5);
		Assert.Equal(12, tally.Total);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-3)]
	public void IgnoresNonPositiveGains(int amount)
	{
		var tally = new MantraTally();
		tally.Add(2);
		tally.Add(amount);
		Assert.Equal(2, tally.Total);
	}
}
