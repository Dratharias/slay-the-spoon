using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EndingMoveBrainTests
{
	[Fact]
	public void Brain_OpensWithTheRulesTurnZeroMove()
	{
		var b = new EndingMoveBrain<HeartMove>(EndingMoveRule.Heart, HeartMove.Debilitate);
		Assert.Equal(HeartMove.Debilitate, b.Advance(1)); Assert.Equal(1, b.Turn);
	}

	[Fact]
	public void Brain_OnlyAdvanceChangesState()
	{
		var b = new EndingMoveBrain<SpearMove>(EndingMoveRule.Spear, SpearMove.BurnStrike);
		b.Advance(0); var (turn, current) = (b.Turn, b.Current); _ = b.Current; _ = b.Turn;
		Assert.Equal((turn, current), (b.Turn, b.Current));
	}

	[Fact]
	public void Brain_ReplaysIdenticallyFromTheSameRolls()
	{
		int[] rolls = [0, 1, 1, 0, 1, 0, 0, 1];
		// Shield turn 0 ignores the previous move, so any initial value works.
		var a = new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Bash);
		var c = new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Bash);
		Assert.Equal(rolls.Select(a.Advance), rolls.Select(c.Advance));
	}

	[Fact]
	public void Heart_FollowsStS1Cycle()
	{
		var b = new EndingMoveBrain<HeartMove>(EndingMoveRule.Heart, HeartMove.Buff);
		HeartMove[] expected = [HeartMove.Debilitate, HeartMove.BloodShots, HeartMove.Echo, HeartMove.Buff,
			HeartMove.Echo, HeartMove.BloodShots, HeartMove.Buff];
		Assert.Equal(expected, new[] { 1, 0, 0, 0, 1, 0, 0 }.Select(b.Advance));
	}

	[Fact]
	public void Shield_FollowsStS1Cycle()
	{
		var b = new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Smash);
		ShieldMove[] expected = [ShieldMove.Fortify, ShieldMove.Bash, ShieldMove.Smash,
			ShieldMove.Bash, ShieldMove.Fortify, ShieldMove.Smash];
		Assert.Equal(expected, new[] { 0, 0, 0, 1, 0, 0 }.Select(b.Advance));
	}

	[Fact]
	public void Spear_FollowsStS1Cycle()
	{
		// Turn 0 is Burn Strike whatever the initial previous move.
		var b = new EndingMoveBrain<SpearMove>(EndingMoveRule.Spear, SpearMove.BurnStrike);
		SpearMove[] expected = [SpearMove.BurnStrike, SpearMove.Skewer, SpearMove.Piercer, SpearMove.BurnStrike,
			SpearMove.Skewer, SpearMove.BurnStrike, SpearMove.Piercer];
		Assert.Equal(expected, new[] { 1, 1, 0, 1, 1, 1, 1 }.Select(b.Advance));
	}
}
