using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class EmeraldRuleTests
{
	[Theory]
	[InlineData(0, 0, EmeraldBuffKind.Strength, 2)]
	[InlineData(2, 0, EmeraldBuffKind.Metallicize, 4)]
	[InlineData(3, 0, EmeraldBuffKind.Regenerate, 3)]
	[InlineData(0, 2, EmeraldBuffKind.Strength, 4)]
	[InlineData(2, 2, EmeraldBuffKind.Metallicize, 8)]
	[InlineData(3, 2, EmeraldBuffKind.Regenerate, 7)]
	[InlineData(1, 1, EmeraldBuffKind.MaxHp, 25)]
	public void Buff_MatchesSts1(int roll, int act, EmeraldBuffKind kind, int amount) =>
		Assert.Equal((kind, amount), EmeraldRule.Buff(roll, act));

	[Theory]
	[InlineData(50, 13)] [InlineData(44, 11)] [InlineData(150, 38)]
	public void MaxHpGain_RoundsHalfUp(int hp, int gain) => Assert.Equal(gain, EmeraldRule.MaxHpGain(hp));

	[Fact]
	public void Pick_IgnoresInputOrder()
	{
		var a = new[] { (4, 9), (1, 6), (2, 6) };
		var b = new[] { (2, 6), (4, 9), (1, 6) };
		Assert.Equal(EmeraldRule.Pick(a, 1), EmeraldRule.Pick(b, 1));
		Assert.Equal((2, 6), EmeraldRule.Pick(a, 1));
	}

	[Fact] public void Pick_NoEliteNoNode() => Assert.Null(EmeraldRule.Pick([], 0));

	[Fact]
	public void MarkAct_KeepsAnAlreadyStoredNode()
	{
		var keys = new Act4Keys();
		Assert.True(EmeraldRule.MarkAct(keys, 1, [(3, 4)], 0));
		Assert.False(EmeraldRule.MarkAct(keys, 1, [(3, 4), (5, 5)], 1));
		Assert.Equal((3, 4), Act4Keys.Decode(keys.Encode()).EmeraldNode(1));
	}

	[Fact]
	public void MarkAct_NothingOnceHeldOrInAct4()
	{
		var keys = new Act4Keys();
		Assert.False(EmeraldRule.MarkAct(keys, 3, [(1, 1)], 0));
		keys.Take(Act4Key.Emerald);
		Assert.False(EmeraldRule.MarkAct(keys, 0, [(1, 1)], 0));
	}

	[Fact]
	public void MarkAct_RepicksWhenTheStoredNodeIsNoLongerAnElite()
	{
		var keys = new Act4Keys();
		EmeraldRule.MarkAct(keys, 0, [(3, 4)], 0);
		Assert.True(EmeraldRule.MarkAct(keys, 0, [(1, 6), (2, 2)], 1));
		Assert.Equal((1, 6), keys.EmeraldNode(0));
	}

	[Fact]
	public void MarkAct_ClearsTheStoredNodeWhenTheNewMapHasNoElite()
	{
		var keys = new Act4Keys();
		EmeraldRule.MarkAct(keys, 0, [(3, 4)], 0);
		Assert.True(EmeraldRule.MarkAct(keys, 0, [], 0));
		Assert.Null(keys.EmeraldNode(0));
	}

	[Fact]
	public void MarkAct_RepickLeavesOtherActsAndHeldKeyAlone()
	{
		var keys = new Act4Keys();
		EmeraldRule.MarkAct(keys, 0, [(3, 4)], 0);
		EmeraldRule.MarkAct(keys, 1, [(2, 2)], 0);
		keys.Take(Act4Key.Emerald);
		Assert.False(EmeraldRule.MarkAct(keys, 0, [(9, 9)], 0));
		Assert.Equal((3, 4), keys.EmeraldNode(0));
		Assert.Equal((2, 2), keys.EmeraldNode(1));
	}
}
