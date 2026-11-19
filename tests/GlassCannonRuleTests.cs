using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class GlassCannonRuleTests
{
	[Theory] [InlineData(0, 1.0)] [InlineData(1, 1.1)] [InlineData(10, 2.0)] [InlineData(15, 2.5)] [InlineData(-2, 1.0)]
	public void MultiplierIsLinear(int stacks, double expected) =>
		Assert.Equal((decimal)expected, GlassCannonRule.DamageMultiplier(stacks));

	[Theory]
	[InlineData(false, false, true, true)]   // Block Skill
	[InlineData(true, false, true, false)]   // Iron Wave
	[InlineData(false, true, true, false)]   // Pillar of Creation, Shroud
	[InlineData(false, false, false, false)] // plain Skill
	public void OnlyNonAttackNonPowerBlockCardsBreak(bool attack, bool power, bool block, bool breaks) =>
		Assert.Equal(breaks, GlassCannonRule.BreaksFrenzy(attack, power, block));

	[Fact]
	public void CardPlayedCountsEveryPlay()
	{
		var s = FrenzyState.Start;
		for (int i = 0; i < 3; i++) s = GlassCannonRule.CardPlayed(s, breaks: false);
		Assert.Equal(3, s.Stacks);
	}

	[Fact]
	public void BlockSkillResetsStacksAndStreak() =>
		Assert.Equal(new FrenzyState(0, 0, true), GlassCannonRule.CardPlayed(new FrenzyState(7, 2, false), breaks: true));

	[Fact]
	public void StreakGivesOneTwoThree()
	{
		var s = FrenzyState.Start;
		var gains = new List<int>();
		for (int turn = 0; turn < 3; turn++)
		{
			s = GlassCannonRule.CardPlayed(s, breaks: false);
			(s, int strength) = GlassCannonRule.TurnEnded(s);
			gains.Add(strength);
		}
		Assert.Equal(new[] { 1, 2, 3 }, gains);
		Assert.Equal(6, gains.Sum());
		Assert.Equal(3, s.Stacks);
	}

	[Fact]
	public void BlockTurnGivesNoStrength()
	{
		var s = new FrenzyState(5, 3, false);
		s = GlassCannonRule.CardPlayed(s, breaks: true);
		(s, int strength) = GlassCannonRule.TurnEnded(s);
		Assert.Equal(0, strength);
		Assert.Equal(0, s.Streak);
		(_, int next) = GlassCannonRule.TurnEnded(s);
		Assert.Equal(1, next);
	}

	[Fact]
	public void TurnEndClearsBlockFlag()
	{
		var (next, _) = GlassCannonRule.TurnEnded(new FrenzyState(0, 0, true));
		Assert.False(next.BlockThisTurn);
	}

	[Fact]
	public void StartRelicsAreVajraAndGirya() => Assert.Equal(new[] { "VAJRA", "GIRYA" }, GlassCannonRule.StartRelics);

	[Fact]
	public void FrailIs99() => Assert.Equal(99, GlassCannonRule.FrailAmount);

	[Fact]
	public void DeadHolderStreakIsFrozen()
	{
		var s = new FrenzyState(4, 2, false);
		var (next, strength) = GlassCannonRule.TurnEnded(s, ownerAlive: false);
		Assert.Equal(0, strength);
		Assert.Equal(s, next);
	}
}
