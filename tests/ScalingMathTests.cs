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

	[Fact]
	public void AllPercents_is_the_25_step_grid()
	{
		Assert.Equal(20, ScalingMath.AllPercents.Count);
		Assert.Equal(25, ScalingMath.AllPercents[0]);
		Assert.Equal(500, ScalingMath.AllPercents[^1]);
		Assert.Contains(ScalingMath.DefaultPercent, ScalingMath.AllPercents);
		for (int i = 1; i < ScalingMath.AllPercents.Count; i++)
		{
			Assert.Equal(ScalingMath.StepPercent, ScalingMath.AllPercents[i] - ScalingMath.AllPercents[i - 1]);
		}
	}
}
