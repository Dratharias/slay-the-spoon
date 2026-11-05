using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Sts1EventMathTests
{
	[Theory]
	[InlineData(10, 13)] // 2.5 rounds up, as libGDX MathUtils.round does
	[InlineData(17, 21)]
	[InlineData(0, 0)]
	public void GoldenIdolAddsTwentyFivePercent(int gold, int expected) =>
		Assert.Equal(expected, Sts1EventMath.GoldenIdolGold(gold));

	[Theory]
	[InlineData(80, 20, 6, 20, 10)]
	[InlineData(75, 18, 6, 19, 9)]
	[InlineData(5, 1, 1, 1, 1)]
	public void EventCostsFollowStS1(int maxHp, int smash, int hide, int altar, int moai)
	{
		Assert.Equal(smash, Sts1EventMath.IdolSmashDamage(maxHp));
		Assert.Equal(hide, Sts1EventMath.IdolHideMaxHpLoss(maxHp));
		Assert.Equal(altar, Sts1EventMath.AltarHpLoss(maxHp));
		Assert.Equal(moai, Sts1EventMath.MoaiMaxHpLoss(maxHp));
	}

	[Fact]
	public void MoaiNeverTakesTheLastPoint() => Assert.Equal(0, Sts1EventMath.MoaiMaxHpLoss(1));
}
