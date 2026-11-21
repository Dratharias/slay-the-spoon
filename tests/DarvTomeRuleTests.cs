using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class DarvTomeRuleTests
{
	private static readonly string[] Relics = ["ASTROLABE", "BLACK_STAR", "CALLING_BELL", "EMPTY_CAGE"];

	[Fact]
	public void SkipsOfferedRelics() =>
		Assert.Equal(2, DarvTomeRule.Replacement(Relics, ["ASTROLABE", "BLACK_STAR"], 0));

	[Fact]
	public void RollWrapsOverFreeRelics() =>
		Assert.Equal(3, DarvTomeRule.Replacement(Relics, ["ASTROLABE", "BLACK_STAR"], 5));

	[Fact]
	public void NoneLeftGivesMinusOne() =>
		Assert.Equal(-1, DarvTomeRule.Replacement(Relics, Relics, 7));
}
