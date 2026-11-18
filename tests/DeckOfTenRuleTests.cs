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

public class DeckOfTenKeepTests
{
	[Theory]
	[InlineData(12, 12, 10)]
	[InlineData(12, 11, 9)]
	[InlineData(15, 3, 0)]
	[InlineData(10, 10, 10)]
	public void KeepsTheRestOfTheTen(int deck, int removable, int keep) => Assert.Equal(keep, DeckOfTenRule.ToKeep(deck, removable));

	[Fact]
	public void NeowOffersItOnOneRollInEleven() =>
		Assert.Equal(1, Enumerable.Range(0, DeckOfTenRule.NeowOdds).Count(DeckOfTenRule.OfferedAtNeow));

	[Fact]
	public void NeowStreamsDifferPerSlot() => Assert.NotEqual(DeckOfTenRule.NeowStream(0), DeckOfTenRule.NeowStream(1));
}
