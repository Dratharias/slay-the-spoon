using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingMoveRuleTests
{
	[Theory] [InlineData(0)] [InlineData(1)]
	public void HeartAlwaysOpensWithDebilitateThenAlternatesAttacks(int roll)
	{
		Assert.Equal(HeartMove.Debilitate, EndingMoveRule.Heart(0, default, roll));
		for (int cycle = 0; cycle < 10; cycle++)
		{
			int turn = 1 + cycle * 3;
			var first = EndingMoveRule.Heart(turn, HeartMove.Buff, roll);
			var second = EndingMoveRule.Heart(turn + 1, first, roll);
			Assert.Contains(first, new[] { HeartMove.BloodShots, HeartMove.Echo });
			Assert.Contains(second, new[] { HeartMove.BloodShots, HeartMove.Echo });
			Assert.NotEqual(first, second);
			Assert.Equal(HeartMove.Buff, EndingMoveRule.Heart(turn + 2, second, roll));
		}
	}

	[Theory] [InlineData(0)] [InlineData(1)]
	public void ShieldAlternatesBashAndFortifyThenSmashes(int roll)
	{
		for (int turn = 0; turn < 30; turn += 3)
		{
			var first = EndingMoveRule.Shield(turn, ShieldMove.Smash, roll);
			var second = EndingMoveRule.Shield(turn + 1, first, roll);
			Assert.NotEqual(first, second);
			Assert.Equal(ShieldMove.Smash, EndingMoveRule.Shield(turn + 2, second, roll));
		}
	}

	[Fact]
	public void SpearAvoidsConsecutiveBurnStrikesAcrossCycles()
	{
		Assert.Equal(SpearMove.BurnStrike, EndingMoveRule.Spear(0, default, 0));
		Assert.Equal(SpearMove.Skewer, EndingMoveRule.Spear(1, SpearMove.BurnStrike, 0));
		Assert.Equal(SpearMove.BurnStrike, EndingMoveRule.Spear(2, SpearMove.Skewer, 1));
		Assert.Equal(SpearMove.Piercer, EndingMoveRule.Spear(3, SpearMove.BurnStrike, 0));
		Assert.Equal(SpearMove.BurnStrike, EndingMoveRule.Spear(3, SpearMove.Piercer, 0));
	}

	[Fact]
	public void BashTakesFocusOnlyFromOrbUsersOnTheRoll()
	{
		Assert.Equal(BashDebuff.Strength, EndingMoveRule.BashDebuffFor(false, 1));
		Assert.Equal(BashDebuff.Strength, EndingMoveRule.BashDebuffFor(true, 0));
		Assert.Equal(BashDebuff.Focus, EndingMoveRule.BashDebuffFor(true, 1));
	}
}
