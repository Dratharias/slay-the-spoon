using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class AppendedMapHeightTests
{
	/// <summary>The bottom (the starting point's line) stays put; everything above it moves down by half.</summary>
	[Fact]
	public void PointsMoveTowardTheBottom()
	{
		Assert.Equal(800f, AppendedMapHeight.Compress(800f), 3);
		Assert.Equal(800f - 2780f * AppendedMapHeight.Factor, AppendedMapHeight.Compress(-1980f), 3);
		Assert.Equal(800f - 60f * AppendedMapHeight.Factor, AppendedMapHeight.Compress(740f), 3);
	}

	[Fact]
	public void HalfHeight()
	{
		Assert.Equal(0.5f, AppendedMapHeight.Factor);
	}

	/// <summary>The scroll range (vanilla −600 to 1800) shrinks with the map; the bottom limit stays.</summary>
	[Fact]
	public void ScrollTopShrinks()
	{
		Assert.Equal(-600f + 2400f * AppendedMapHeight.Factor, AppendedMapHeight.ScrollTop, 3);
		Assert.True(AppendedMapHeight.ScrollTop < 1800f);
	}
}
