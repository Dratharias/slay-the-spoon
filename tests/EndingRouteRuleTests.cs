using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingRouteRuleTests
{
	[Fact]
	public void ShouldAppend_RequiresAct4RunAndAllKeys()
	{
		Assert.True(EndingRouteRule.ShouldAppend(true, true, 3));
		Assert.False(EndingRouteRule.ShouldAppend(true, false, 3));
		Assert.False(EndingRouteRule.ShouldAppend(false, true, 3));
	}

	[Fact]
	public void ShouldAppend_NeverAddsASecondEndingAct() => Assert.False(EndingRouteRule.ShouldAppend(true, true, 4));

	[Fact]
	public void ShouldAppend_IgnoresRunsWithOtherActCounts() => Assert.False(EndingRouteRule.ShouldAppend(true, true, 2));
}
