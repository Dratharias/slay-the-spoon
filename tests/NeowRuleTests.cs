using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class NeowRuleTests
{
	[Fact]
	public void NotTicked_KeepsVanillaCount()
	{
		Assert.Equal(3, NeowRule.VisibleModifierCount(3, blessingTicked: false, anyNeowOption: false, forceBlessings: false));
		Assert.Equal(3, NeowRule.VisibleModifierCount(3, blessingTicked: false, anyNeowOption: true, forceBlessings: true));
	}

	[Fact]
	public void Ticked_WithoutModifierOptions_ShowsBlessings()
	{
		Assert.Equal(0, NeowRule.VisibleModifierCount(2, blessingTicked: true, anyNeowOption: false, forceBlessings: false));
	}

	[Fact]
	public void Ticked_WithModifierOptions_ShowsThemFirstThenBlessings()
	{
		Assert.Equal(2, NeowRule.VisibleModifierCount(2, blessingTicked: true, anyNeowOption: true, forceBlessings: false));
		Assert.Equal(0, NeowRule.VisibleModifierCount(2, blessingTicked: true, anyNeowOption: true, forceBlessings: true));
	}
}
