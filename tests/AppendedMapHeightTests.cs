using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class AppendedMapHeightTests
{
	[Theory]
	[InlineData(3)]
	[InlineData(4)]
	public void ShortMapsClampDragAndReopenPositions(int rows)
	{
		float top = AppendedMapHeight.ScrollTop(rows);
		Assert.Equal(top, AppendedMapHeight.ClampScroll(1800f, top));
		Assert.Equal(-600f, AppendedMapHeight.ClampScroll(-1200f, top));
		float middle = (-600f + top) / 2f;
		Assert.Equal(middle, AppendedMapHeight.ClampScroll(middle, top));
	}

	[Theory]
	[InlineData(3, 1080f)]
	[InlineData(4, 1080f)]
	[InlineData(3, 900f)]
	[InlineData(4, 900f)]
	public void BossIconFitsBelowTopBarAtScrollLimit(int rows, float viewportHeight)
	{
		// BossMapPoint has centered anchors: parenting adds half the Points height.
		float laidOutY = -1980f + viewportHeight / 2f;
		float bossY = AppendedMapHeight.BossPointY(laidOutY, rows);
		float screenY = bossY + AppendedMapHeight.ScrollTop(rows);
		// The 350px placeholder is centered at y=153 relative to the boss control.
		// Include its 1.05 hover scale, so the whole image clears the top bar.
		float iconTop = screenY + 153f - 175f * 1.05f;
		float iconBottom = screenY + 153f + 175f * 1.05f;
		Assert.True(iconTop >= 80f, $"Icon top {iconTop} is behind the top bar");
		Assert.True(iconBottom <= viewportHeight, $"Icon bottom {iconBottom} is off screen");
		Assert.Equal(laidOutY + 1800f, screenY, 2);
	}

	[Fact]
	public void BossKeepsParentAnchorTranslation()
	{
		Assert.Equal(AppendedMapHeight.BossY(3) + 540f,
			AppendedMapHeight.BossPointY(-1980f + 540f, 3), 2);
	}

	/// <summary>Rows get the spacing of a solo act 3 (Glory: 14 grid rows over vanilla's 2325 px).</summary>
	[Fact]
	public void RowsUseActThreeSpacing()
	{
		Assert.Equal(2325f / 13f, AppendedMapHeight.RowSpacing, 3);
	}

	/// <summary>A 3-row map spaced 1162.5 px apart: row 2 at 740 − 2325 (plus jitter) moves to 740 − 2 × spacing.</summary>
	[Fact]
	public void PointsKeepTheirJitterOnTheNewRow()
	{
		float vanillaDist = 2325f / 2f;
		Assert.Equal(740f + 12f - 2f * AppendedMapHeight.RowSpacing,
			AppendedMapHeight.PointY(740f + 12f - 2f * vanillaDist, 2, vanillaDist), 3);
		Assert.Equal(800f, AppendedMapHeight.PointY(800f, 0, vanillaDist), 3);
	}

	/// <summary>In Glory the boss (−1980) sits 395 px above the last row (740 − 13 × spacing); the same gap is kept.</summary>
	[Fact]
	public void BossKeepsVanillaGapAboveLastRow()
	{
		Assert.Equal(-1980f, AppendedMapHeight.BossY(14), 2);
		float lastRow = 740f - 2f * AppendedMapHeight.RowSpacing;
		Assert.Equal(lastRow - (740f - 13f * AppendedMapHeight.RowSpacing + 1980f), AppendedMapHeight.BossY(3), 2);
	}

	/// <summary>The scroll stops where the boss is shown as high as vanilla shows it at 1800; never under the bottom.</summary>
	[Fact]
	public void ScrollTopFollowsTheBoss()
	{
		Assert.Equal(1800f, AppendedMapHeight.ScrollTop(14), 2);
		Assert.Equal(1800f - (AppendedMapHeight.BossY(3) + 1980f), AppendedMapHeight.ScrollTop(3), 2);
		Assert.True(AppendedMapHeight.ScrollTop(1) >= -600f);
	}
}
