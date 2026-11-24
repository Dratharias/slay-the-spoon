using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WitchingPactRuleTests
{
	[Theory]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	[InlineData(false, false, false)]
	[InlineData(false, true, false)]
	public void OfferedInCursedDeckWithoutOvercursed(bool cursedDeck, bool overcursed, bool offered) =>
		Assert.Equal(offered, WitchingPactRule.Offered(cursedDeck, overcursed));

	[Fact]
	public void ReplacesTheBaneWhenThereIsOne() =>
		Assert.Equal(WitchingPactDeal.ReplaceBane, WitchingPactRule.Deal(hasAscendersBane: true));

	[Fact]
	public void AddsTheCurseWithoutABane() =>
		Assert.Equal(WitchingPactDeal.AddCurse, WitchingPactRule.Deal(hasAscendersBane: false));

	[Fact]
	public void ThreeRelicsPerPlayerStream()
	{
		Assert.Equal(3, WitchingPactRule.Size);
		Assert.NotEqual(WitchingPactRule.Stream(0), WitchingPactRule.Stream(1));
	}
}
