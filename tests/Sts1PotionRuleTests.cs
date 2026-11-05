using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Sts1PotionRuleTests
{
	[Theory]
	[InlineData(CardSetMode.Sts1Only, true)]
	[InlineData(CardSetMode.Hybrid, false)]
	[InlineData(CardSetMode.Sts2Only, false)]
	public void AncientPotion_OnlyInSts1OnlyRuns(CardSetMode mode, bool offered) =>
		Assert.Equal(offered, Sts1PotionRule.Offered(mode));
}
