using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Sts1MathTests
{
	[Theory]
	[InlineData(0, 12)]
	[InlineData(1, 16)]
	[InlineData(2, 21)]
	[InlineData(3, 27)]
	[InlineData(10, 97)]
	public void SearingBlowDamage_MatchesSts1(int level, int expected)
	{
		Assert.Equal(expected, Sts1Math.SearingBlowDamage(level));
	}

	[Theory]
	[InlineData(-3, true, true, true)]
	[InlineData(0, true, true, false)]
	[InlineData(5, true, true, false)]
	[InlineData(-3, false, true, false)]
	[InlineData(-3, true, false, false)]
	public void MasterfulStab_counts_owner_hp_loss_while_in_hand_draw_or_discard(int hpDelta, bool ownerLostIt,
		bool inHandDrawOrDiscard, bool expected)
	{
		Assert.Equal(expected, Sts1Math.MasterfulStabCounts(hpDelta, ownerLostIt, inHandDrawOrDiscard));
	}
}
