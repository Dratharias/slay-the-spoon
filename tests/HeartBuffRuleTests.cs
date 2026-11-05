using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class HeartBuffRuleTests
{
	[Fact]
	public void BuffsEscalateThenRepeatPlusFifty()
	{
		Assert.Equal((HeartBuffKind.Artifact, 2), HeartBuffRule.Extra(0));
		Assert.Equal((HeartBuffKind.BeatOfDeath, 1), HeartBuffRule.Extra(1));
		Assert.Equal((HeartBuffKind.PainfulStabs, 1), HeartBuffRule.Extra(2));
		Assert.Equal((HeartBuffKind.Strength, 10), HeartBuffRule.Extra(3));
		Assert.Equal((HeartBuffKind.Strength, 50), HeartBuffRule.Extra(4));
		Assert.Equal((HeartBuffKind.Strength, 50), HeartBuffRule.Extra(9));
	}

	[Fact]
	public void NegativeBuffCountIsRejected() =>
		Assert.Throws<ArgumentOutOfRangeException>(() => HeartBuffRule.Extra(-1));

	[Theory]
	[InlineData(0, 2)] [InlineData(5, 2)] [InlineData(-3, 5)] [InlineData(-1, 3)]
	public void BaseStrengthFirstLiftsNegativeStrengthToZero(int current, int expectedGain) =>
		Assert.Equal(expectedGain, HeartBuffRule.StrengthGain(current));
}
