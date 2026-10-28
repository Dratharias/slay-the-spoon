using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ScalingMathTests
{
	[Theory]
	[InlineData(100, 1, 125)]
	[InlineData(100, -1, 75)]
	[InlineData(25, -1, 25)]
	[InlineData(500, 1, 500)]
	[InlineData(110, 1, 125)] // off-grid values snap to the grid
	[InlineData(110, -1, 100)]
	public void Step_moves_one_notch_and_clamps(int percent, int direction, int expected)
	{
		Assert.Equal(expected, ScalingMath.Step(percent, direction));
	}

	[Theory]
	[InlineData(40, 100, 40)]
	[InlineData(40, 150, 60)]
	[InlineData(41, 150, 62)] // 61.5 rounds up
	[InlineData(1, 25, 1)]
	[InlineData(3, 25, 1)]
	[InlineData(500, 500, 2500)]
	public void ScaleHp_rounds_half_up_with_minimum_one(int hp, int percent, int expected)
	{
		Assert.Equal(expected, ScalingMath.ScaleHp(hp, percent));
	}

	[Fact]
	public void DamageMultiplier_default_is_exactly_one()
	{
		Assert.Equal(1m, ScalingMath.DamageMultiplier(ScalingMath.DefaultPercent));
	}

	[Fact]
	public void DamageMultiplier_quarter()
	{
		Assert.Equal(0.25m, ScalingMath.DamageMultiplier(25));
	}
}
