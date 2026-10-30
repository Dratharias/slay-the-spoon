using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherPowerMathTests
{
	[Theory]
	[InlineData(1, 1, 2)]
	[InlineData(2, 1, 3)]
	[InlineData(2, 2, 4)]
	public void DevaNextGain_AddsTheStackAmount(int gain, int amount, int expected)
	{
		Assert.Equal(expected, WatcherPowerMath.DevaNextGain(gain, amount));
	}

	[Theory]
	[InlineData(2, 1, -1)]
	[InlineData(1, 1, -1)]
	[InlineData(1, 2, -1)] // never below 0: only the cost that is left is removed
	[InlineData(0, 1, 0)]
	[InlineData(3, 2, -2)]
	public void EstablishmentReduction_StopsAtZero(int cost, int amount, int expected)
	{
		Assert.Equal(expected, WatcherPowerMath.EstablishmentReduction(cost, amount));
	}

	[Theory]
	[InlineData(false, 10, true)]
	[InlineData(false, 12, true)]
	[InlineData(false, 9, false)]
	[InlineData(true, 10, false)]
	public void DevotionEntersDivinity_NeedsNoMantraAndTen(bool hasMantra, int amount, bool expected)
	{
		Assert.Equal(expected, WatcherPowerMath.DevotionEntersDivinity(hasMantra, amount));
	}
}
