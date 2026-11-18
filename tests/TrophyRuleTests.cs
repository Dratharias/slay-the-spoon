using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class TrophyRuleTests
{
	[Theory]
	[InlineData(14, 1, 1)]
	[InlineData(15, 1, 2)]
	[InlineData(5, 3, 2)]
	[InlineData(10, 3, 3)]
	[InlineData(6, 0, 1)]
	public void CostFollowsTotalDamage(int damage, int hits, int cost) => Assert.Equal(cost, TrophyRule.Cost(damage, hits));

	[Theory]
	[InlineData(16, 4)]
	[InlineData(10, 3)]
	[InlineData(1, 1)]
	[InlineData(0, 1)]
	public void UpgradeAddsAQuarterRoundedUp(int damage, int bonus) => Assert.Equal(bonus, TrophyRule.UpgradeBonus(damage));
}
