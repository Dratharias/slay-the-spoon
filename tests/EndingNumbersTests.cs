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
}
