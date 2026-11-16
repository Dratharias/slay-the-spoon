using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonRuleTests
{
	[Fact]
	public void ShouldInsertWhenEverythingLines_Up() =>
		Assert.True(SpoonRule.ShouldInsert(true, true, true, [310, 80], false));

	[Fact]
	public void ShouldNotInsertOtherwise()
	{
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [299, 250], false));
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

	[Fact]
	public void FightersAreThoseAtThreshold() =>
		Assert.Equal([0, 2], SpoonRule.Fighters([300, 299, 450]));

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
	[InlineData(1, 1, 2000)]
	[InlineData(1, 2, 1000)]
	[InlineData(2, 2, 2000)]
	[InlineData(1, 3, 667)]
	public void BossHp(int f, int p, int expected) => Assert.Equal(expected, SpoonRule.BossHp(f, p));

	[Theory]
	[InlineData(0, SpoonRule.SpoonMove.SetTable)]
	[InlineData(1, SpoonRule.SpoonMove.Bouchee)]
	[InlineData(2, SpoonRule.SpoonMove.Banquet)]
	[InlineData(3, SpoonRule.SpoonMove.Ladle)]
	[InlineData(4, SpoonRule.SpoonMove.SetTable)]
	[InlineData(7, SpoonRule.SpoonMove.Ladle)]
	public void CycleOfFour(int turn, SpoonRule.SpoonMove move) => Assert.Equal(move, SpoonRule.Move(turn));

	[Theory]
	[InlineData(2000, 2000, 0, 0)]
	[InlineData(2009, 2000, 0, 1)]
	[InlineData(2010, 2000, 0, 2)]
	[InlineData(1996, 2000, 0, 0)]
	[InlineData(1995, 2000, 0, -1)]
	[InlineData(1985, 2000, 0, -3)]
	[InlineData(2324, 2000, 324, 0)]
	[InlineData(2312, 2000, 324, -2)]
	[InlineData(2128, 2000, 100, 5)]
	public void StrengthFollowsMaxHpButNotGrowth(int hp, int start, int grown, int expected) =>
		Assert.Equal(expected, SpoonRule.StrengthFor(hp, start, grown));

	[Theory]
	[InlineData(1, 60)]
	[InlineData(2, 70)]
	[InlineData(3, 80)]
	[InlineData(4, 90)]
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
	[InlineData(0, 0)]
	[InlineData(5, 1)]
	[InlineData(1, 1)]
	public void SpoonTakesAppetiteBackOnlyWhenItPiercesBlock(int unblocked, int taken) =>
		Assert.Equal(taken, SpoonRule.StolenBack(unblocked));

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

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(5, true)]
	[InlineData(300, true)]
	public void CanGiftWhileTheGiverHasMoreThanOneMaxHp(int ownerMaxHp, bool expected) =>
		Assert.Equal(expected, SpoonRule.CanGift(ownerMaxHp));

	[Theory]
	[InlineData(0, 0)]
	[InlineData(1, 0)]
	[InlineData(2, 1)]
	[InlineData(5, 4)]
	[InlineData(6, 5)]
	[InlineData(300, 5)]
	public void GiftAmountLeavesTheGiverOneMaxHp(int ownerMaxHp, int expected) =>
		Assert.Equal(expected, SpoonRule.GiftAmount(ownerMaxHp));

	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(6)]
	[InlineData(300)]
	public void GiftNeverDropsTheGiverBelowOneMaxHp(int ownerMaxHp) =>
		Assert.Equal(Math.Max(1, ownerMaxHp - SpoonRule.GiftMaxHp), ownerMaxHp - SpoonRule.GiftAmount(ownerMaxHp));

	[Theory]
	[InlineData(300, 295, 5)]
	[InlineData(6, 1, 5)]
	[InlineData(3, 1, 2)]
	[InlineData(10, 10, 0)]
	[InlineData(10, 12, 0)]
	[InlineData(20, 5, 5)]
	public void GiftTransfersWhatTheGiverLost(int before, int after, int expected) =>
		Assert.Equal(expected, SpoonRule.GiftTransferred(before, after));

	[Fact]
	public void JuiceSlot()
	{
		Assert.Equal(-1, SpoonRule.JuiceSlot(3, true, 5));
		Assert.Equal(-1, SpoonRule.JuiceSlot(0, false, 5));
		Assert.Equal(2, SpoonRule.JuiceSlot(3, false, 5));
		Assert.Equal(1, SpoonRule.JuiceSlot(3, false, -5));
	}

	[Fact]
	public void FightersAtFightFallsBackToEveryone()
	{
		Assert.Equal([1], SpoonRule.FightersAtFight([120, 300, 299]));
		Assert.Equal([0, 1], SpoonRule.FightersAtFight([120, 80]));
	}
}
