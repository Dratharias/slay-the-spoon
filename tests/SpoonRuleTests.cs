using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonRuleTests
{
	[Theory]
	[InlineData(-1, 0)]
	[InlineData(0, 0)]
	[InlineData(3, 3)]
	[InlineData(40, 40)]
	public void GrowthKeepsAllUnblockedDamage(int damage, int expected) =>
		Assert.Equal(expected, SpoonRule.GrowthFromDamage(damage));

	[Theory]
	[InlineData("LOOMING_FRUIT", 275)]
	[InlineData("BIG_MUSHROOM", 275)]
	[InlineData("NUTRITIOUS_OYSTER", 225)]
	[InlineData("STONE_HUMIDIFIER", 225)]
	[InlineData("DARKSTONE_PERIAPT", 225)]
	[InlineData("CHOSEN_CHEESE", 225)]
	[InlineData("FAKE_MANGO", 175)]
	[InlineData("ANCHOR", null)]
	public void PromotedRelicsHaveOrdinaryTierPrices(string entry, int? expected) =>
		Assert.Equal(expected, SpoonRule.ExtraRelicPrice(entry));

	[Fact]
	public void ShouldInsertWhenEverythingLines_Up() =>
		Assert.True(SpoonRule.ShouldInsert(true, true, true, [470, 80], false));

	[Theory]
	[InlineData(1, 300)]
	[InlineData(2, 550)]
	[InlineData(3, 750)]
	[InlineData(4, 900)]
	[InlineData(0, 300)]
	[InlineData(5, 900)]
	public void TeamThresholdByPlayerCount(int players, int threshold) =>
		Assert.Equal(threshold, SpoonRule.TeamThreshold(players));

	[Theory]
	[InlineData(new[] { 300 }, true)]
	[InlineData(new[] { 299 }, false)]
	[InlineData(new[] { 275, 275 }, true)]
	[InlineData(new[] { 500, 49 }, false)]
	[InlineData(new[] { 250, 250, 250 }, true)]
	[InlineData(new[] { 225, 225, 225, 224 }, false)]
	[InlineData(new[] { 400, 200, 150, 150 }, true)]
	public void TeamReachesTheThresholdTogether(int[] maxHps, bool expected) =>
		Assert.Equal(expected, SpoonRule.ShouldInsert(true, true, true, maxHps, false));

	[Fact]
	public void ShouldNotInsertOtherwise()
	{
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [299, 250], false));
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [299], false));
		Assert.False(SpoonRule.ShouldInsert(true, true, false, [310], false));
		Assert.False(SpoonRule.ShouldInsert(false, true, true, [310], false));
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [310], true));
		Assert.False(SpoonRule.ShouldInsert(true, false, true, [310], false));
	}

	[Fact]
	public void InsertIndexByActShape()
	{
		Assert.Equal(3, SpoonRule.InsertIndex(3, false));
		Assert.Equal(3, SpoonRule.InsertIndex(4, true));
		Assert.Equal(-1, SpoonRule.InsertIndex(4, false));
		Assert.Equal(-1, SpoonRule.InsertIndex(5, true));
	}

	[Theory]
	[InlineData(0, 1.0)]
	[InlineData(1, 1.05)]
	[InlineData(5, 1.25)]
	[InlineData(-2, 0.9)]
	[InlineData(-20, 0.0)]
	[InlineData(-30, 0.0)]
	public void AppetiteMultiplier(int stacks, double expected) =>
		Assert.Equal((decimal)expected, SpoonRule.AppetiteMultiplier(stacks));

	[Theory]
	[InlineData(0)]
	[InlineData(4)]
	[InlineData(12)]
	public void SatedSpoonHitsAtBase(int stacks) => Assert.Equal(1m, SpoonRule.AppetiteMultiplier(stacks, sated: true));

	[Theory]
	[InlineData(SpoonRule.SpoonMove.SetTable, true)]
	[InlineData(SpoonRule.SpoonMove.Bouchee, false)]
	[InlineData(SpoonRule.SpoonMove.Banquet, false)]
	[InlineData(SpoonRule.SpoonMove.Ladle, false)]
	public void AppetiteGrowsOnlyOnTurnOne(SpoonRule.SpoonMove move, bool gains) =>
		Assert.Equal(gains, SpoonRule.GainsAppetite(move));

	[Theory]
	[InlineData(1, 2)]
	[InlineData(2, 4)]
	[InlineData(4, 8)]
	[InlineData(0, 2)]
	public void AppetiteGainScalesWithPlayers(int players, int gain) => Assert.Equal(gain, SpoonRule.AppetiteGain(players));

	[Theory]
	[InlineData(0, SpoonRule.SpoonMove.SetTable)]
	[InlineData(1, SpoonRule.SpoonMove.Bouchee)]
	[InlineData(2, SpoonRule.SpoonMove.Banquet)]
	[InlineData(3, SpoonRule.SpoonMove.Ladle)]
	[InlineData(4, SpoonRule.SpoonMove.SetTable)]
	[InlineData(7, SpoonRule.SpoonMove.Ladle)]
	public void CycleOfFour(int turn, SpoonRule.SpoonMove move) => Assert.Equal(move, SpoonRule.Move(turn));

	[Theory]
	[InlineData(2000, 2000, 0)]
	[InlineData(2004, 2000, 0)]
	[InlineData(2005, 2000, 1)]
	[InlineData(2009, 2000, 1)]
	[InlineData(2010, 2000, 2)]
	[InlineData(1996, 2000, 0)]
	[InlineData(1995, 2000, -1)]
	[InlineData(1985, 2000, -3)]
	[InlineData(2324, 2000, 64)]
	[InlineData(2312, 2000, 62)]
	[InlineData(2128, 2000, 25)]
	[InlineData(1010, 1000, 2)]
	public void StrengthFollowsAllMaxHpChangesFromCombatBaseline(int hp, int start, int expected) =>
		Assert.Equal(expected, SpoonRule.StrengthFor(hp, start));

	[Fact]
	public void StrengthReturnsToZeroAndBecomesNegativeBelowBaseline()
	{
		const int baseline = 2000;
		int previous = 0;
		int applied = 0;
		foreach (int hp in new[] { 2324, 2312, 2000, 1985, 2000 })
		{
			int target = SpoonRule.StrengthFor(hp, baseline);
			applied += target - previous;
			previous = target;
			Assert.Equal((hp - baseline) / 5, applied);
		}
		Assert.Equal(0, applied);
	}

	[Theory]
	[InlineData(1, 30)]
	[InlineData(2, 40)]
	[InlineData(3, 50)]
	[InlineData(4, 60)]
	public void BanquetHealByPlayers(int players, int heal) => Assert.Equal(heal, SpoonRule.BanquetHeal(players));

	[Theory]
	[InlineData(2000, 12)]
	[InlineData(13, 12)]
	[InlineData(10, 9)]
	[InlineData(1, 0)]
	public void BallerTakeIsCapped(int hp, int take) => Assert.Equal(take, SpoonRule.BallerTake(hp));

	[Fact]
	public void BallerTakeHonoursTheUpgrade() => Assert.Equal(15, SpoonRule.BallerTake(2000, 15));

	[Theory]
	[InlineData(0, 0)]
	[InlineData(20, 0)]
	[InlineData(24, 0)]
	[InlineData(25, 1)]
	[InlineData(29, 1)]
	[InlineData(30, 2)]
	[InlineData(45, 5)]
	public void ExtraSpoonsFromTwentyFive(int deck, int extra) => Assert.Equal(extra, SpoonRule.ExtraSpoons(deck));

	[Fact]
	public void KitHasOneOfEachOfTheFourSpoons() =>
		Assert.Equal(4, Enum.GetValues<SpoonRule.Cutlery>().Distinct().Count());

	[Theory]
	[InlineData(0, SpoonRule.Cutlery.Teaspoon)]
	[InlineData(4, SpoonRule.Cutlery.Teaspoon)]
	[InlineData(6, SpoonRule.Cutlery.Skimmer)]
	[InlineData(-1, SpoonRule.Cutlery.Baller)]
	public void ExtraPickCoversTheKit(int roll, SpoonRule.Cutlery expected) => Assert.Equal(expected, SpoonRule.ExtraPick(roll));

	[Theory]
	[InlineData(350, 35)]
	[InlineData(304, 30)]
	[InlineData(9, 0)]
	public void WoodenSpoonHitsTenPercentOfMaxHp(int maxHp, int damage) => Assert.Equal(damage, SpoonRule.WoodenDamage(maxHp));

	[Theory]
	[InlineData(false, 0)]
	[InlineData(true, 1)]
	public void SpoonTakesAppetiteBackOnHitRegardlessOfBlock(bool hit, int taken) =>
		Assert.Equal(taken, SpoonRule.StolenBack(hit));

	[Theory]
	[InlineData(115, 350, 28)]
	[InlineData(0, 350, 0)]
	[InlineData(3, 350, 0)]
	[InlineData(80, 10, 9)]
	[InlineData(80, 1, 0)]
	public void LadleStealsAQuarterOfThePiercingDamageAsMaxHp(int unblocked, int maxHp, int stolen) =>
		Assert.Equal(stolen, SpoonRule.LadleMaxHpSteal(unblocked, maxHp));

	[Fact]
	public void BanquetHitsLightly() => Assert.InRange(SpoonRule.BanquetDamage, 1, SpoonRule.BoucheeDamage / 2);

	[Fact]
	public void ExtraRelicsTable()
	{
		Assert.Equal(7, SpoonRule.ExtraRelics.Select(r => r.Entry).Distinct().Count());
		Assert.All(SpoonRule.ExtraRelics, r => Assert.Contains(r.Deque, new[] { "Common", "Uncommon", "Rare" }));
	}

	[Fact]
	public void JuiceSlot()
	{
		Assert.Equal(-1, SpoonRule.JuiceSlot(3, true, 5));
		Assert.Equal(-1, SpoonRule.JuiceSlot(0, false, 5));
		Assert.Equal(2, SpoonRule.JuiceSlot(3, false, 5));
		Assert.Equal(1, SpoonRule.JuiceSlot(3, false, -5));
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(1, 1)]
	[InlineData(4, 4)]
	[InlineData(-1, 0)]
	public void OrdinaryMonstersStealOneAppetitePerHit(int hits, int stolen) => Assert.Equal(stolen, SpoonRule.StolenOnHits(hits));
}
