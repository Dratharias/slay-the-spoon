using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class VanillaActIndexTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(1, 1)]
	[InlineData(2, 2)]
	public void For_KeepsVanillaActs(int actIndex, int expected) => Assert.Equal(expected, VanillaActIndex.For(actIndex));

	[Theory]
	[InlineData(3)]
	[InlineData(4)]
	public void For_MapsTheEndingToAct3(int actIndex) => Assert.Equal(2, VanillaActIndex.For(actIndex));
}
