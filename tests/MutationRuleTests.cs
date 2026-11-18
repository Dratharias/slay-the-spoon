using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MutationRuleTests
{
	[Fact]
	public void FirstActNeverMutates() => Assert.False(MutationRule.Mutates(0, -1));

	[Fact]
	public void LaterActsMutateOnce()
	{
		Assert.True(MutationRule.Mutates(1, -1));
		Assert.False(MutationRule.Mutates(1, 1));
		Assert.True(MutationRule.Mutates(2, 1));
	}

	[Fact]
	public void StreamsDifferPerActAndPlayer()
	{
		Assert.NotEqual(MutationRule.Stream(1, 0), MutationRule.Stream(2, 0));
		Assert.NotEqual(MutationRule.Stream(1, 0), MutationRule.Stream(1, 1));
	}
}
