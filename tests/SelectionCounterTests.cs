using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SelectionCounterTests
{
	[Theory]
	[InlineData(1, 10, null)]
	[InlineData(2, 10, 2)]
	[InlineData(5, 3, 3)]
	[InlineData(5, 1, null)]
	[InlineData(0, 10, null)]
	public void Limit_ShowsOnlyMultiPicks(int max, int cards, int? expected) =>
		Assert.Equal(expected, SelectionCounter.Limit(max, cards));

	[Fact]
	public void Compose_AppendsOnANewLine() =>
		Assert.Equal("Choose 2 cards.\nSelected: 1 / 2", SelectionCounter.Compose("Choose 2 cards.", "Selected: 1 / 2"));

	[Fact]
	public void Compose_KeepsTheCounterInsideATrailingCenterBlock() =>
		Assert.Equal("[center]Enchant\nSelected: 0 / 3[/center]", SelectionCounter.Compose("[center]Enchant[/center]", "Selected: 0 / 3"));
}
