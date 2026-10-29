using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Sts1MathTests
{
	[Theory]
	[InlineData(0, 12)]
	[InlineData(1, 16)]
	[InlineData(2, 21)]
	[InlineData(3, 27)]
	[InlineData(10, 97)]
	public void SearingBlowDamage_MatchesSts1(int level, int expected)
	{
		Assert.Equal(expected, Sts1Math.SearingBlowDamage(level));
	}
}
