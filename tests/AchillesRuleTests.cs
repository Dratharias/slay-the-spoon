using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class AchillesRuleTests
{
	[Theory] [InlineData(0, 0)] [InlineData(1, 1)] [InlineData(2, 2)] [InlineData(3, 3)] [InlineData(-1, 0)]
	public void DexterityByAct(int act, int dex) => Assert.Equal(dex, AchillesRule.DexterityFor(act));

	[Theory] [InlineData(0, 0)] [InlineData(4, 0)] [InlineData(5, 1)] [InlineData(14, 2)] [InlineData(-3, 0)]
	public void BlockBonus(int block, int bonus) => Assert.Equal(bonus, AchillesRule.BlockBonus(block));

	[Fact]
	public void StartRelicsAreTheThreeBlockRelics() =>
		Assert.Equal(new[] { "ANCHOR", "ODDLY_SMOOTH_STONE", "GORGET" }, AchillesRule.StartRelics);

	[Theory] [InlineData(true, 1, true)] [InlineData(true, 0, false)] [InlineData(false, 5, false)] [InlineData(false, 0, false)]
	public void HeelTriggersOnlyOnUnblockedPoweredAttack(bool powered, int unblocked, bool kills) =>
		Assert.Equal(kills, AchillesRule.HeelKills(powered, unblocked));
}
