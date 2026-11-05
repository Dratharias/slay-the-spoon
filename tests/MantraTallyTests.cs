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

	[Fact]
	public void EachKeyKeepsItsOwnTally()
	{
		var tallies = new MantraTallies<object>();
		object first = new(), second = new();
		tallies.Record(first, 4);
		tallies.Record(first, 3);
		tallies.Record(second, 5);
		Assert.Equal(7, tallies.Gained(first));
		Assert.Equal(5, tallies.Gained(second));
	}

	/// <summary>A new combat state is a new key, so the count starts over.</summary>
	[Fact]
	public void UnseenKeyStartsAtZero()
	{
		var tallies = new MantraTallies<object>();
		tallies.Record(new object(), 9);
		Assert.Equal(0, tallies.Gained(new object()));
	}
}
