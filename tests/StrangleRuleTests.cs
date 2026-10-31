using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class StrangleRuleTests
{
	[Theory]
	[InlineData(true, 2, true)]
	[InlineData(true, 4, true)]
	[InlineData(true, 1, false)]
	[InlineData(false, 2, false)]
	public void Allies_count_only_in_a_co_op_custom_run(bool custom, int players, bool expected)
	{
		Assert.Equal(expected, StrangleRule.CountsAllies(custom, players));
	}
}
