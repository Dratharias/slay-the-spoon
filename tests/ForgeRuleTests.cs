using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ForgeRuleTests
{
	[Theory]
	[InlineData(0, 0, 9, 1)]
	[InlineData(1, 0, 9, 2)]
	[InlineData(1, 1, 9, 3)]
	[InlineData(3, 3, 9, 7)]
	[InlineData(5, 5, 9, 9)]
	[InlineData(0, 0, 1, 1)]
	[InlineData(2, 2, 3, 3)]
	public void FusedLevelAddsUpAndClamps(int kept, int sacrificed, int cap, int expected) =>
		Assert.Equal(expected, ForgeRule.FusedLevel(kept, sacrificed, cap));

	[Fact]
	public void CannotFuseAtTheCap()
	{
		Assert.False(ForgeRule.CanFuseInto(1, 1, true));
		Assert.True(ForgeRule.CanFuseInto(0, 1, true));
		Assert.True(ForgeRule.CanFuseInto(8, 9, true));
		Assert.False(ForgeRule.CanFuseInto(9, 9, true));
		Assert.False(ForgeRule.CanFuseInto(0, 9, false));
	}

	[Theory]
	[InlineData(ForgeRarity.Common, false, 0, 25)]
	[InlineData(ForgeRarity.Uncommon, false, 0, 50)]
	[InlineData(ForgeRarity.Rare, false, 0, 75)]
	[InlineData(ForgeRarity.Common, true, 0, 50)]
	[InlineData(ForgeRarity.Rare, true, 0, 100)]
	[InlineData(ForgeRarity.Rare, true, 2, 150)]
	[InlineData(ForgeRarity.Uncommon, false, 3, 125)]
	public void DuplicatePrice(ForgeRarity rarity, bool inflation, int purchases, int expected) =>
		Assert.Equal(expected, ForgeRule.DuplicatePrice(rarity, inflation, purchases));

	[Theory]
	[InlineData(2, -1, 3)]
	[InlineData(1, -1, 2)]
	[InlineData(0, -1, 1)]
	[InlineData(1, -2, 2)]
	[InlineData(3, -2, 3)]
	[InlineData(2, 0, 1)]
	public void CostCapStopsAtZero(int costAfterFirst, int delta, int expected) =>
		Assert.Equal(expected, ForgeRule.CostCap(costAfterFirst, delta));

	[Fact]
	public void CostAtFloorsAtZero()
	{
		Assert.Equal(2, ForgeRule.CostAt(2, -1, 1));
		Assert.Equal(0, ForgeRule.CostAt(2, -1, 3));
		Assert.Equal(0, ForgeRule.CostAt(2, -1, 9));
	}

	[Fact]
	public void ParsesRows()
	{
		var rows = ForgeRule.ParseCurves("""
			{ "_comment": "x",
			  "A": { "cap": 4, "perLevel": { "StrengthPower": 1 }, "note": "n" },
			  "B": { "fusable": false },
			  "C": { "costPerLevel": -1 } }
			""");
		Assert.Equal(3, rows.Count);
		Assert.Equal(4, rows["A"].Cap);
		Assert.Equal(1m, rows["A"].PerLevel!["StrengthPower"]);
		Assert.False(rows["B"].Fusable);
		Assert.Equal(-1, rows["C"].CostPerLevel);
		Assert.True(rows["C"].Fusable);
	}

	[Fact]
	public void RejectsBadRows()
	{
		Assert.Throws<FormatException>(() => ForgeRule.ParseCurves("""{ "A": { "cap": 12 } }"""));
		Assert.Throws<FormatException>(() => ForgeRule.ParseCurves("""{ "A": { "costPerLevel": 1 } }"""));
	}

	[Fact]
	public void CommittedTableParses()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "mod", "SlaySpoon.json"))) dir = dir.Parent;
		var rows = ForgeRule.ParseCurves(File.ReadAllText(Path.Combine(dir!.FullName, "data", "forge", "curves.json")));
		Assert.True(rows.Count > 100);
		Assert.Equal(5, rows["STS1_LIMIT_BREAK"].Cap);
		Assert.Equal(1m, rows["STS1_LIMIT_BREAK"].PerLevel!["StrengthPower"]);
	}

	[Theory]
	[InlineData(5, ForgeBonus.None)]
	[InlineData(6, ForgeBonus.Retain)]
	[InlineData(7, ForgeBonus.CostDown)]
	[InlineData(8, ForgeBonus.None)]
	[InlineData(9, ForgeBonus.Innate)]
	public void LadderBonusPerLevel(int level, ForgeBonus bonus) => Assert.Equal(bonus, ForgeRule.BonusAt(level));

	[Fact]
	public void LadderAccumulates()
	{
		Assert.Equal(ForgeBonus.None, ForgeRule.BonusesUpTo(5));
		Assert.Equal(ForgeBonus.Retain | ForgeBonus.CostDown, ForgeRule.BonusesUpTo(8));
		Assert.Equal(ForgeBonus.Retain | ForgeBonus.CostDown | ForgeBonus.Innate, ForgeRule.BonusesUpTo(9));
	}

	[Fact]
	public void ParsesBeyond()
	{
		var rows = ForgeRule.ParseCurves("""{ "A": { "cap": 3, "beyond": ["Damage"] }, "B": { "cap": 5 } }""");
		Assert.Equal(new[] { "Damage" }, rows["A"].Beyond);
		Assert.Null(rows["B"].Beyond);
	}
}
