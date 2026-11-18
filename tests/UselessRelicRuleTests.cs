using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class UselessRelicRuleTests
{
	[Fact]
	public void AkabekoAfterItsFlashCanBeThrown() => Assert.True(UselessRelicRule.CanThrow("AKABEKO", true, false, false));

	[Theory]
	[InlineData("AKABEKO", false, false, false)]
	[InlineData("AKABEKO", true, true, false)]
	[InlineData("AKABEKO", true, false, true)]
	[InlineData("BURNING_BLOOD", true, false, false)]
	public void OtherwiseItStays(string id, bool flashed, bool melted, bool thrown) =>
		Assert.False(UselessRelicRule.CanThrow(id, flashed, melted, thrown));

	[Fact]
	public void IdsAreUpperSnakeCase() =>
		Assert.All(UselessRelicRule.OncePerCombat, id => Assert.Matches("^[A-Z_]+$", id));
}
