using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ScryMathTests
{
	[Theory]
	[InlineData(3, 8, false, 3)]
	[InlineData(8, 3, false, 3)]
	[InlineData(8, 3, true, 3)]
	[InlineData(3, 8, true, 5)]
	[InlineData(3, 4, true, 4)]
	[InlineData(3, 0, false, 0)]
	[InlineData(3, 0, true, 0)]
	[InlineData(0, 8, true, 2)]
	[InlineData(0, 8, false, 0)]
	[InlineData(-1, 8, true, 0)]
	[InlineData(3, -1, false, 0)]
	[InlineData(int.MaxValue, 8, true, 8)]
	public void CardsToShow_RespectsPileAndBonus(int amount, int pile, bool goldenEye, int expected)
	{
		Assert.Equal(expected, ScryMath.CardsToShow(amount, pile, goldenEye));
	}
}
