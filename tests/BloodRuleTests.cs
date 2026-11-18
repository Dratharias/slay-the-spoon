using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class BloodRuleTests
{
	[Theory]
	[InlineData(80, 395)]
	[InlineData(1, 0)]
	[InlineData(0, 0)]
	public void SpendableGoldKeepsOneHp(int hp, int gold) => Assert.Equal(gold, BloodRule.SpendableGold(hp));

	[Theory]
	[InlineData(75, 80, 15)]
	[InlineData(76, 80, 16)]
	[InlineData(1, 80, 1)]
	[InlineData(0, 80, 0)]
	[InlineData(999, 10, 9)]
	public void PaymentsRoundUpAndNeverKill(int gold, int hp, int cost) => Assert.Equal(cost, BloodRule.HpCost(gold, hp));

	[Theory]
	[InlineData(10, 2)]
	[InlineData(4, 0)]
	[InlineData(-3, 0)]
	public void FangHealsAFifth(int damage, int heal) => Assert.Equal(heal, BloodRule.Leech(damage));
}
