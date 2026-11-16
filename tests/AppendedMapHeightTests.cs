using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class AppendedMapHeightTests
{
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
