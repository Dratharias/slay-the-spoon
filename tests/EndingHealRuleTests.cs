using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingHealRuleTests
{
	[Fact]
	public void HealsTheMissingHp() => Assert.Equal(40m, EndingHealRule.Amount(80, 40, false));

	[Fact]
	public void WearyTraveler_HealsEightyPercentOfIt() => Assert.Equal(32m, EndingHealRule.Amount(80, 40, true));

	[Fact]
	public void FullHp_HealsNothing() => Assert.Equal(0m, EndingHealRule.Amount(80, 80, true));
}
