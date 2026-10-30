using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class StanceMathTests
{
	[Fact]
	public void Wrath_DoublesDealtAndReceived()
	{
		Assert.Equal(2m, StanceMath.DealtMultiplier(Stance.Wrath));
		Assert.Equal(2m, StanceMath.ReceivedMultiplier(Stance.Wrath));
	}

	[Fact]
	public void Divinity_TriplesDealtOnly()
	{
		Assert.Equal(3m, StanceMath.DealtMultiplier(Stance.Divinity));
		Assert.Equal(1m, StanceMath.ReceivedMultiplier(Stance.Divinity));
	}

	[Fact]
	public void CalmExit_GivesTwo()
	{
		Assert.Equal(2, StanceMath.EnergyOnExit(Stance.Calm, violetLotus: false));
	}

	[Fact]
	public void CalmExit_WithVioletLotus_GivesThree()
	{
		Assert.Equal(3, StanceMath.EnergyOnExit(Stance.Calm, violetLotus: true));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void NeutralExit_GivesNothing(bool violetLotus)
	{
		Assert.Equal(0, StanceMath.EnergyOnExit(Stance.Neutral, violetLotus));
		// Violet Lotus only adds to leaving Calm; no other stance gives energy on exit either.
		Assert.Equal(0, StanceMath.EnergyOnExit(Stance.Wrath, violetLotus));
		Assert.Equal(0, StanceMath.EnergyOnExit(Stance.Divinity, violetLotus));
		// Neutral and Calm multiply nothing; only Divinity gives energy on entry.
		Assert.Equal(1m, StanceMath.DealtMultiplier(Stance.Neutral));
		Assert.Equal(1m, StanceMath.ReceivedMultiplier(Stance.Neutral));
		Assert.Equal(1m, StanceMath.DealtMultiplier(Stance.Calm));
		Assert.Equal(1m, StanceMath.ReceivedMultiplier(Stance.Calm));
		Assert.Equal(0, StanceMath.EnergyOnEnter(Stance.Neutral));
		Assert.Equal(0, StanceMath.EnergyOnEnter(Stance.Calm));
		Assert.Equal(0, StanceMath.EnergyOnEnter(Stance.Wrath));
		Assert.Equal(3, StanceMath.EnergyOnEnter(Stance.Divinity));
	}

	[Fact]
	public void Mantra_TenEntersDivinity()
	{
		Assert.Equal((0, true), StanceMath.AddMantra(7, 3));
		Assert.Equal((0, true), StanceMath.AddMantra(0, 10));
	}

	[Fact]
	public void Mantra_TwelveKeepsTwo()
	{
		Assert.Equal((2, true), StanceMath.AddMantra(9, 3));
		Assert.Equal((2, true), StanceMath.AddMantra(0, 12));
	}

	[Fact]
	public void Mantra_NineStays()
	{
		Assert.Equal((9, false), StanceMath.AddMantra(6, 3));
		Assert.Equal((9, false), StanceMath.AddMantra(0, 9));
	}
}
