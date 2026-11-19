using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ForgeTitleColorsTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	public void VanillaLevelsKeepTheGameColors(int level) => Assert.Null(ForgeTitleColors.For(level, TitleBanner.Common));

	[Fact]
	public void EveryLevelFromTwoHasItsOwnColor()
	{
		var fills = Enumerable.Range(2, 8).Select(l => ForgeTitleColors.For(l, TitleBanner.Common)!.Value.Fill).ToList();
		Assert.Equal(fills.Count, fills.Distinct().Count());
	}

	[Fact]
	public void OnlyDiamondShimmers()
	{
		for (int level = 2; level <= 9; level++)
			Assert.Equal(level == 9, ForgeTitleColors.For(level, TitleBanner.Rare)!.Value.Shimmer);
		Assert.True(ForgeTitleColors.For(12, TitleBanner.Common)!.Value.Shimmer);
	}

	[Theory]
	[InlineData(3, TitleBanner.Rare, true)]
	[InlineData(3, TitleBanner.Uncommon, false)]
	[InlineData(7, TitleBanner.Uncommon, true)]
	[InlineData(7, TitleBanner.Rare, false)]
	[InlineData(8, TitleBanner.Common, false)]
	public void ClashingColorsGetADarkOutline(int level, TitleBanner banner, bool clash)
	{
		Assert.Equal(clash, ForgeTitleColors.Clashes(level, banner));
		var plain = ForgeTitleColors.For(level, TitleBanner.Other)!.Value;
		var shown = ForgeTitleColors.For(level, banner)!.Value;
		Assert.Equal(clash, shown != plain);
		if (clash) Assert.Equal("140C00", shown.Outline);
	}

	[Fact]
	public void TheFrameTurnsAncientStepByStep()
	{
		Assert.Equal(0f, ForgeTitleColors.AncientBlend(1));
		Assert.Equal(0f, ForgeTitleColors.Gloss(1));
		for (int level = 3; level <= 9; level++)
		{
			Assert.True(ForgeTitleColors.AncientBlend(level) > ForgeTitleColors.AncientBlend(level - 1));
			Assert.True(ForgeTitleColors.Gloss(level) > ForgeTitleColors.Gloss(level - 1));
		}
		Assert.Equal(1f, ForgeTitleColors.AncientBlend(9));
		Assert.Equal(1f, ForgeTitleColors.AncientBlend(12));
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(5, false)]
	[InlineData(6, true)]
	[InlineData(9, true)]
	public void TheBannerSwapsAtOnce(int level, bool ancient) => Assert.Equal(ancient, ForgeTitleColors.AncientBanner(level));
}
