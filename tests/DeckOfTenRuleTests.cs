using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class DeckOfTenRuleTests
{
	[Theory]
	[InlineData(10, 10, 0)]
	[InlineData(11, 11, 1)]
	[InlineData(12, 12, 2)]
	[InlineData(12, 1, 1)]
	[InlineData(5, 5, 0)]
	public void DropsTheExcessItCan(int deck, int removable, int drop) => Assert.Equal(drop, DeckOfTenRule.ToDrop(deck, removable));
}
