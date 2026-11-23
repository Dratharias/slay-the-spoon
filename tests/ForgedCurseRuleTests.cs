using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ForgedCurseRuleTests
{
	private static readonly CurseRow BadLuck = new("BAD_LUCK", "HpLoss", 13, 20, "HpLoss", null, 0);
	private static readonly CurseRow Decay = new("DECAY", "Damage", 2, 4, "Damage", null, 0);
	private static readonly CurseRow Regret = new("REGRET", "HpLossPerHandCard", 1, 2, null, null, 0);
	private static readonly CurseRow Clumsy = new("CLUMSY", "None", 0, 0, null, null, 0);

	[Theory]
	[InlineData(0, 13)]
	[InlineData(1, 20)]
	[InlineData(2, 27)]
	[InlineData(9, 76)]
	public void BadLuckGrowsByItsUpgradeStep(int level, int expected) =>
		Assert.Equal(expected, ForgedCurseRule.AmountAt(BadLuck, level));

	[Fact]
	public void DecayGrowsByTwo()
	{
		Assert.Equal(18, ForgedCurseRule.AmountAt(Decay, 8));
		Assert.Equal(20, ForgedCurseRule.AmountAt(Decay, 9));
	}

	[Theory]
	[InlineData(0, 5)]
	[InlineData(1, 12)]
	[InlineData(2, 15)]
	[InlineData(3, 19)]
	[InlineData(9, 40)]
	public void GrimoireForNoEffectCurses(int level, int expected) =>
		Assert.Equal(expected, ForgedCurseRule.GrimoireAt(Clumsy, level));

	[Fact]
	public void GrimoireStaysFiveForPlayedCurses()
	{
		foreach (int level in new[] { 0, 1, 5, 9 }) Assert.Equal(5, ForgedCurseRule.GrimoireAt(BadLuck, level));
		Assert.Equal(40, ForgedCurseRule.GrimoireAt(null, 9));
	}

	[Theory]
	[InlineData(0, 3)]
	[InlineData(1, 4)]
	[InlineData(8, 11)]
	[InlineData(9, 12)]
	public void NormalityAllowsOneMoreCardPerLevel(int level, int expected) =>
		Assert.Equal(expected, ForgedCurseRule.NormalityLimitAt(level));

	[Theory]
	[InlineData(0, 5)]
	[InlineData(1, 3)]
	[InlineData(2, 2)]
	[InlineData(3, 1)]
	[InlineData(9, 1)]
	public void GuiltyLeavesSooner(int level, int expected) =>
		Assert.Equal(expected, ForgedCurseRule.GuiltyCombatsAt(level));

	[Theory]
	[InlineData(2, 0)]
	[InlineData(3, 1)]
	[InlineData(5, 1)]
	[InlineData(6, 2)]
	[InlineData(8, 2)]
	[InlineData(9, 3)]
	public void DefectCountFollowsLevel(int level, int expected) =>
		Assert.Equal(expected, ForgedCurseRule.DefectCount(level));

	private static int Never(int n) => throw new InvalidOperationException("no draw expected");

	[Fact]
	public void MergeKeepsKeptFirst() =>
		Assert.Equal(CurseDefect.Late | CurseDefect.Heavy,
			ForgedCurseRule.Merge(CurseDefect.Late, CurseDefect.Heavy | CurseDefect.Echo, 6, Never));

	[Fact]
	public void MergeSkipsIncompatibleSacrificedDefect() =>
		Assert.Equal(CurseDefect.Odd | CurseDefect.Heavy,
			ForgedCurseRule.Merge(CurseDefect.Odd, CurseDefect.Even | CurseDefect.Heavy, 6, Never));

	[Fact]
	public void MergeDrawsMissing()
	{
		var a = ForgedCurseRule.Merge(CurseDefect.None, CurseDefect.None, 9, _ => 0);
		var b = ForgedCurseRule.Merge(CurseDefect.None, CurseDefect.None, 9, _ => 0);
		Assert.Equal(a, b);
		Assert.Equal(3, ForgedCurseRule.All.Count(d => a.HasFlag(d)));
		Assert.False(a.HasFlag(CurseDefect.Odd) && a.HasFlag(CurseDefect.Even));
		Assert.Equal(CurseDefect.None, ForgedCurseRule.Merge(CurseDefect.None, CurseDefect.None, 2, Never));
	}

	[Fact]
	public void MergeNeverPairsOddAndEvenWhateverTheRoll()
	{
		for (int seed = 0; seed < 200; seed++)
		{
			var rng = new Random(seed);
			var set = ForgedCurseRule.Merge(CurseDefect.None, CurseDefect.None, 9, n => rng.Next(n));
			Assert.False(set.HasFlag(CurseDefect.Odd) && set.HasFlag(CurseDefect.Even));
			Assert.Equal(3, ForgedCurseRule.All.Count(d => set.HasFlag(d)));
		}
	}

	[Fact]
	public void MergeNeverShrinksBelowKept() =>
		Assert.Equal(CurseDefect.Odd | CurseDefect.Late,
			ForgedCurseRule.Merge(CurseDefect.Odd | CurseDefect.Late, CurseDefect.None, 3, Never));

	[Fact]
	public void BlocksPlayByTurnAndCardsPlayed()
	{
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Odd, 1, 5));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Odd, 3, 5));
		Assert.False(ForgedCurseRule.BlocksPlay(CurseDefect.Odd, 2, 5));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Even, 2, 5));
		Assert.False(ForgedCurseRule.BlocksPlay(CurseDefect.Even, 1, 5));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Late, 1, 5));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Late, 2, 5));
		Assert.False(ForgedCurseRule.BlocksPlay(CurseDefect.Late, 3, 5));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Demanding, 4, 0));
		Assert.True(ForgedCurseRule.BlocksPlay(CurseDefect.Demanding, 4, 1));
		Assert.False(ForgedCurseRule.BlocksPlay(CurseDefect.Demanding, 4, 2));
		Assert.False(ForgedCurseRule.BlocksPlay(CurseDefect.None, 1, 0));
	}

	[Fact]
	public void HeavyOnlyWhenPlayable()
	{
		Assert.Equal(3, ForgedCurseRule.CostWithDefects(CurseDefect.Heavy, 1, true));
		Assert.Equal(1, ForgedCurseRule.CostWithDefects(CurseDefect.Heavy, 1, false));
		Assert.Equal(0, ForgedCurseRule.CostWithDefects(CurseDefect.None, 0, true));
	}

	[Fact]
	public void SmallNumbers()
	{
		Assert.Equal(6, ForgedCurseRule.EchoAmount(13));
		Assert.Equal(0, ForgedCurseRule.EchoAmount(1));
		Assert.Equal(7, ForgedCurseRule.BloodyHp(7));
		Assert.Equal(2, ForgedCurseRule.SpitefulTimes(CurseDefect.Spiteful | CurseDefect.Odd));
		Assert.Equal(1, ForgedCurseRule.SpitefulTimes(CurseDefect.None));
	}

	[Fact]
	public void GateStopsOrdinaryUpgradePastOne()
	{
		Assert.Equal(1, ForgedCurseRule.MaxLevelFor(false, false, 0));
		Assert.Equal(1, ForgedCurseRule.MaxLevelFor(false, true, 0));
		Assert.Equal(1, ForgedCurseRule.MaxLevelFor(true, false, 0));
		Assert.Equal(9, ForgedCurseRule.MaxLevelFor(true, true, 0));
	}

	[Fact]
	public void LoadKeepsLevelAboveOne()
	{
		Assert.Equal(7, ForgedCurseRule.LoadLevel(7, true));
		Assert.Equal(1, ForgedCurseRule.LoadLevel(7, false));
		Assert.Equal(9, ForgedCurseRule.LoadLevel(12, true));
		Assert.Equal(0, ForgedCurseRule.LoadLevel(0, true));
	}

	[Fact]
	public void PlayedAmountFollowsLevel()
	{
		Assert.Equal(76, CurseRule.PlayedAmount(BadLuck, 9, 0, 0));
		Assert.Equal(4 * ForgedCurseRule.AmountAt(Regret, 3), CurseRule.PlayedAmount(Regret, 3, 4, 0));
		Assert.Equal(CurseRule.PlayedAmount(BadLuck, true, 0, 0), CurseRule.PlayedAmount(BadLuck, 1, 0, 0));
		Assert.Equal(40, CurseRule.GrimoireDamageFor(Clumsy, 9));
		Assert.Equal(CurseRule.GrimoireDamageFor(Clumsy, true), CurseRule.GrimoireDamageFor(Clumsy, 1));
	}

	[Fact]
	public void DefectStreamName() =>
		Assert.Equal("forged_curse_defect_12_3_0", ForgedCurseRule.DefectStream(12, 3, 0));
}
