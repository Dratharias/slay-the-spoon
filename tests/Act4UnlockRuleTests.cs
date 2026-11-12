using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Act4UnlockRuleTests
{
	[Theory]
	[InlineData(true, 10, 2, true)]   // reaches Act 3 at A10
	[InlineData(true, 10, 1, false)]  // only Act 2
	[InlineData(true, 9, 2, false)]   // below A10
	[InlineData(false, 10, 2, false)] // Custom or Daily
	[InlineData(true, 10, 3, true)]   // later acts too
	public void ShouldUnlock(bool standard, int ascension, int actIndex, bool expected) => Assert.Equal(expected, Act4UnlockRule.ShouldUnlock(standard, ascension, actIndex));

	[Theory]
	[InlineData(0, false)]
	[InlineData(4, false)]
	[InlineData(5, true)]
	[InlineData(10, true)]
	public void UnlocksWithAscensionFiveAvailable(int maximum, bool expected) =>
		Assert.Equal(expected, Act4UnlockRule.ShouldUnlock(maximum));

	[Fact]
	public void InStandardRun_NeedsTheUnlockOnceReleased()
	{
		Assert.True(Act4Access.InStandardRun(true));
		Assert.False(Act4Access.InStandardRun(false));
	}
}
