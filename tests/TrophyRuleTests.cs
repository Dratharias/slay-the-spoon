using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class TrophyRuleTests
{
	[Theory]
	[InlineData(14, 1, 1)]
	[InlineData(15, 1, 2)]
	[InlineData(5, 3, 2)]
	[InlineData(10, 3, 3)]
	[InlineData(6, 0, 1)]
	public void CostFollowsTotalDamage(int damage, int hits, int cost) => Assert.Equal(cost, TrophyRule.Cost(damage, hits));

	[Theory]
	[InlineData(16, 4)]
	[InlineData(10, 3)]
	[InlineData(1, 1)]
	[InlineData(0, 1)]
	public void UpgradeAddsAQuarterRoundedUp(int damage, int bonus) => Assert.Equal(bonus, TrophyRule.UpgradeBonus(damage));

	[Fact]
	public void MoveSuffixIsDroppedFirst() => Assert.Equal(
		new[] { "TERROR_EEL.moves.THRASH.title", "TERROR_EEL.moves.THRASH_MOVE.title" },
		TrophyRule.MoveTitleKeys("TERROR_EEL", "THRASH_MOVE"));

	[Fact]
	public void VariantNumberIsDroppedToo() => Assert.Equal(
		new[] { "SKULKING_COLONY.moves.ZOOM.title", "SKULKING_COLONY.moves.ZOOM_MOVE.title", "SKULKING_COLONY.moves.ZOOM_MOVE_2.title" },
		TrophyRule.MoveTitleKeys("SKULKING_COLONY", "ZOOM_MOVE_2"));

	[Fact]
	public void PlainMoveIsUsedAsIs() =>
		Assert.Equal(new[] { "TERROR_EEL.moves.CRASH.title" }, TrophyRule.MoveTitleKeys("TERROR_EEL", "CRASH"));

	[Fact]
	public void PortraitIsLowerCase() =>
		Assert.Equal("res://SlaySpoon/images/cards/trophies/terror_eel.png", TrophyRule.PortraitPath("TERROR_EEL"));
}
