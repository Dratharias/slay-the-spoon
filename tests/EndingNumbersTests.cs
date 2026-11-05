using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingNumbersTests
{
	[Fact]
	public void ShieldScalesAtToughDeadlyAndDoubleBoss()
	{
		Assert.Equal(110, EndingNumbers.ShieldHp(7)); Assert.Equal(125, EndingNumbers.ShieldHp(8));
		Assert.Equal(12, EndingNumbers.ShieldBash(8)); Assert.Equal(14, EndingNumbers.ShieldBash(9));
		Assert.Equal(34, EndingNumbers.ShieldSmash(8)); Assert.Equal(38, EndingNumbers.ShieldSmash(9));
		Assert.Equal(1, EndingNumbers.ShieldArtifact(9)); Assert.Equal(2, EndingNumbers.ShieldArtifact(10));
		Assert.Null(EndingNumbers.ShieldSmashFixedBlock(9)); Assert.Equal(99, EndingNumbers.ShieldSmashFixedBlock(10));
	}

	[Fact]
	public void SpearScalesAtToughDeadlyAndDoubleBoss()
	{
		Assert.Equal(160, EndingNumbers.SpearHp(7)); Assert.Equal(180, EndingNumbers.SpearHp(8));
		Assert.Equal(5, EndingNumbers.SpearBurnStrike(8)); Assert.Equal(6, EndingNumbers.SpearBurnStrike(9));
		Assert.Equal(10, EndingNumbers.SpearSkewer(9)); Assert.Equal(3, EndingNumbers.SpearSkewerHits(8)); Assert.Equal(4, EndingNumbers.SpearSkewerHits(9));
		Assert.False(EndingNumbers.SpearBurnsToDrawPile(9)); Assert.True(EndingNumbers.SpearBurnsToDrawPile(10));
		Assert.Equal(2, EndingNumbers.SpearArtifact(10));
	}

	[Fact]
	public void HeartScalesAtToughDeadlyAndDoubleBoss()
	{
		Assert.Equal(750, EndingNumbers.HeartHp(7)); Assert.Equal(800, EndingNumbers.HeartHp(8));
		Assert.Equal(2, EndingNumbers.HeartBloodShot(9)); Assert.Equal(12, EndingNumbers.HeartBloodShotHits(8)); Assert.Equal(15, EndingNumbers.HeartBloodShotHits(9));
		Assert.Equal(40, EndingNumbers.HeartEcho(8)); Assert.Equal(45, EndingNumbers.HeartEcho(9));
		Assert.Equal(1, EndingNumbers.HeartBeatOfDeath(9)); Assert.Equal(2, EndingNumbers.HeartBeatOfDeath(10));
	}

	/// <summary>Each value at A0, just below its threshold and at it.</summary>
	[Theory]
	[InlineData("ShieldHp", 8, 110, 125)] [InlineData("ShieldArtifact", 10, 1, 2)] [InlineData("ShieldBash", 9, 12, 14)]
	[InlineData("ShieldSmash", 9, 34, 38)] [InlineData("SpearHp", 8, 160, 180)] [InlineData("SpearArtifact", 10, 1, 2)]
	[InlineData("SpearBurnStrike", 9, 5, 6)] [InlineData("SpearSkewerHits", 9, 3, 4)] [InlineData("HeartHp", 8, 750, 800)]
	[InlineData("HeartBeatOfDeath", 10, 1, 2)] [InlineData("HeartBloodShotHits", 9, 12, 15)] [InlineData("HeartEcho", 9, 40, 45)]
	public void ThresholdsHoldOnBothSides(string name, int threshold, int below, int atOrAbove)
	{
		var f = typeof(EndingNumbers).GetMethod(name)!;
		int At(int a) => (int)f.Invoke(null, [a])!;
		Assert.Equal(below, At(0));
		Assert.Equal(below, At(threshold - 1));
		Assert.Equal(atOrAbove, At(threshold));
		Assert.Equal(atOrAbove, At(10));
	}

	[Fact]
	public void FlatValuesIgnoreAscension()
	{
		Assert.Equal(10, EndingNumbers.SpearSkewer(0)); Assert.Equal(2, EndingNumbers.HeartBloodShot(0));
		Assert.False(EndingNumbers.SpearBurnsToDrawPile(0));
		Assert.Null(EndingNumbers.ShieldSmashFixedBlock(0));
		Assert.Equal(0, EndingNumbers.ShieldSmashBlock(0, []));
		Assert.Equal(34, EndingNumbers.ShieldSmashBlock(9, [12, 34, 20]));
		Assert.Equal(99, EndingNumbers.ShieldSmashBlock(10, [12]));
	}
}
