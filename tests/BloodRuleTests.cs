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

	[Theory]
	[InlineData(33, 35)]
	[InlineData(35, 35)]
	[InlineData(1, 5)]
	[InlineData(0, 0)]
	public void PaidGoldIsWholeHp(int price, int paid) => Assert.Equal(paid, BloodRule.PaidGold(price));

	/// <summary>Bot run 2026-11-20 (King Midas + Blood): 60 gold, a 33 gold card passed the check but cost 35, leaving 25: both players ruined.</summary>
	[Fact]
	public void MidasCheckUsesTheRealBloodCost()
	{
		Assert.True(MidasRule.CanSpend(60, 33, 25));
		Assert.False(MidasRule.CanSpend(60, BloodRule.PaidGold(33), 25));
	}
}
