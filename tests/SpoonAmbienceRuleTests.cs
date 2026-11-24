using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonAmbienceRuleTests
{
	[Fact]
	public void FullSlidersGiveTheAmbienceLevel() => Assert.Equal(SpoonAmbienceRule.Level, SpoonAmbienceRule.Volume(1f, 1f), 5);

	[Fact]
	public void FollowsTheSquaredSfxSlider() => Assert.Equal(0.25f * SpoonAmbienceRule.Level, SpoonAmbienceRule.Volume(1f, 0.5f), 5);

	[Fact]
	public void MutedSfxIsSilent() => Assert.Equal(0f, SpoonAmbienceRule.Volume(1f, 0f));

	[Fact]
	public void StaysUnderTheMusic() => Assert.True(SpoonAmbienceRule.Level < 0.5f);
}
