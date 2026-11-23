using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MidasRuleTests
{
	private static List<int> Kept(params (string Key, bool Enabled)[] options) =>
		MidasRule.KeptOptions(options.Select(o => o.Key).ToList(), options.Select(o => o.Enabled).ToList()).ToList();

	[Fact]
	public void KeepsOnlyGoldOption() =>
		Assert.Equal([1], Kept(("LOST_WISP.pages.INITIAL.options.CLAIM", true), ("LOST_WISP.pages.INITIAL.options.SEARCH", true)));

	[Fact]
	public void KeepsBothWhenBothGiveGold() =>
		Assert.Equal([0, 1], Kept(("SUNKEN_TREASURY.pages.INITIAL.options.FIRST_CHEST", true),
			("SUNKEN_TREASURY.pages.INITIAL.options.SECOND_CHEST", true)));

	[Fact]
	public void ColossalFlowerKeepsDiggingButDropsCore()
	{
		Assert.Equal([0, 1], Kept(("COLOSSAL_FLOWER.pages.REACH_DEEPER_1.options.EXTRACT_CURRENT_PRIZE_2", true),
			("COLOSSAL_FLOWER.pages.REACH_DEEPER_1.options.REACH_DEEPER_2", true)));
		Assert.Equal([0], Kept(("COLOSSAL_FLOWER.pages.REACH_DEEPER_2.options.EXTRACT_INSTEAD", true),
			("COLOSSAL_FLOWER.pages.REACH_DEEPER_2.options.POLLINOUS_CORE", true)));
	}

	[Fact]
	public void UnlistedEventStaysVanilla() =>
		Assert.Equal([0, 1], Kept(("NEOW.pages.INITIAL.options.A", true), ("NEOW.pages.INITIAL.options.B", true)));

	[Fact]
	public void PageWithoutEnabledGoldOptionStaysVanilla()
	{
		Assert.Equal([0, 1], Kept(("STS1_MOAI_HEAD.pages.INITIAL.options.JUMP", true),
			("STS1_MOAI_HEAD.pages.INITIAL.options.OFFER_LOCKED", false)));
		Assert.Equal([0], Kept(("THE_LANTERN_KEY.pages.KEEP_THE_KEY.options.FIGHT", true)));
	}

	[Fact]
	public void DisabledGoldOptionDoesNotCount() =>
		Assert.Equal([0, 1], Kept(("TRASH_HEAP.pages.INITIAL.options.DIVE_IN", true), ("TRASH_HEAP.pages.INITIAL.options.GRAB", false)));

	[Fact]
	public void EveryListedEventHasAKeptOption()
	{
		foreach (string ev in MidasRule.GoldEvents)
			Assert.Contains(MidasRule.GoldOptionPrefixes, p => p.StartsWith(ev + ".pages."));
	}

	[Fact]
	public void PickIsInRange()
	{
		for (int r = 0; r < 50; r++) Assert.InRange(MidasRule.Pick(7, r), 0, 6);
		Assert.Equal(-1, MidasRule.Pick(0, 3));
	}

	[Fact]
	public void RestPriceEscalatesPerFire()
	{
		Assert.Equal(50, MidasRule.RestPrice(0));
		Assert.Equal(100, MidasRule.RestPrice(1));
		Assert.Equal(150, MidasRule.RestPrice(2));
	}

	[Theory]
	[InlineData(3900, 25, 390)]
	[InlineData(300, 25, 30)]
	[InlineData(100, 25, 25)]
	[InlineData(100, 60, 60)]
	[InlineData(20, 25, 20)]
	[InlineData(0, 25, 0)]
	public void MeltIsTenPercentAtLeastTheFloor(int gold, int floor, int melt) => Assert.Equal(melt, MidasRule.Melt(gold, floor));

	[Theory]
	[InlineData(0, 0, 25)]
	[InlineData(15, 1, 25)]
	[InlineData(330, 5, 66)]
	[InlineData(1644, 7, 234)]
	[InlineData(-10, 3, 25)]
	public void FloorIsTheAverageIncome(int gained, int floors, int floor) => Assert.Equal(floor, MidasRule.MeltFloor(gained, floors));

	[Theory]
	[InlineData(0, 25, true)]
	[InlineData(25, 25, true)]
	[InlineData(26, 25, false)]
	[InlineData(117, 117, true)]
	[InlineData(118, 117, false)]
	[InlineData(3900, 117, false)]
	public void RuinedWhenTheMeltEmptiesThePurse(int gold, int floor, bool ruined) => Assert.Equal(ruined, MidasRule.Ruined(gold, floor));

	[Theory]
	[InlineData(false, 3900, 117, MidasRule.Travel.Pay)]
	[InlineData(false, 25, 25, MidasRule.Travel.PayAndDie)]
	[InlineData(false, 1, 25, MidasRule.Travel.PayAndDie)]
	[InlineData(true, 0, 25, MidasRule.Travel.Revive)]
	[InlineData(true, 3900, 117, MidasRule.Travel.Revive)]
	public void DeadPlayersComeBackInsteadOfPaying(bool dead, int gold, int floor, MidasRule.Travel travel) =>
		Assert.Equal(travel, MidasRule.OnTravel(dead, gold, floor));

	[Theory]
	[InlineData(100, 74, 25, true)]
	[InlineData(100, 75, 25, false)]
	[InlineData(100, 101, 25, false)]
	[InlineData(200, 80, 117, true)]
	[InlineData(200, 90, 117, false)]
	public void SpendingMustLeaveEnoughToSurvive(int gold, int price, int floor, bool ok) =>
		Assert.Equal(ok, MidasRule.CanSpend(gold, price, floor));

	[Fact]
	public void PricesRiseTenPercentPerPurchase()
	{
		Assert.Equal(1m, MidasRule.PriceFactor(0));
		Assert.Equal(1.1m, MidasRule.PriceFactor(1));
		Assert.Equal(1.5m, MidasRule.PriceFactor(5));
	}

	[Fact]
	public void LeaveOnlyWhenNothingIsAffordable()
	{
		Assert.True(MidasRule.LeaveOnly(new[] { false, false, false }));
		Assert.False(MidasRule.LeaveOnly(new[] { false, true }));
		Assert.True(MidasRule.LeaveOnly(Array.Empty<bool>()));
	}

	[Fact]
	public void VanillaMidasDoublesTheCosts()
	{
		Assert.Equal(1, MidasRule.Scale(false));
		Assert.Equal(2, MidasRule.Scale(true));
		Assert.Equal(100, MidasRule.RestPrice(0, 2));
		Assert.Equal(200, MidasRule.RestPrice(1, 2));
		Assert.Equal(200, MidasRule.Melt(1000, 50, 2));
		Assert.Equal(50, MidasRule.MeltFloor(0, 0, 2));
		Assert.Equal(50, MidasRule.MeltFloor(100, 10, 2));
		Assert.Equal(80, MidasRule.MeltFloor(800, 10, 2));
		Assert.Equal(1.2m, MidasRule.PriceFactor(1, 2));
		Assert.False(MidasRule.CanSpend(100, 50, 50, 2));
		Assert.True(MidasRule.CanSpend(100, 50, 50, 1) == !MidasRule.Ruined(50, 50, 1));
	}

	[Theory]
	[InlineData(175, 131)]
	[InlineData(225, 168)]
	[InlineData(275, 206)]
	[InlineData(200, 150)]
	public void SalePriceIsThreeQuartersRoundedDown(int cost, int sale) => Assert.Equal(sale, MidasRule.SalePrice(cost));

	[Theory]
	[InlineData(131, 164)]
	[InlineData(168, 210)]
	[InlineData(206, 258)]
	[InlineData(150, 188)]
	public void ResaleFloorIsSalePlusAQuarterRoundedUp(int sale, int floor) => Assert.Equal(floor, MidasRule.ResaleFloor(sale));

	[Theory]
	[InlineData(MidasRelicRarity.Common, false, true)]
	[InlineData(MidasRelicRarity.Uncommon, false, true)]
	[InlineData(MidasRelicRarity.Rare, false, true)]
	[InlineData(MidasRelicRarity.Shop, false, true)]
	[InlineData(MidasRelicRarity.Starter, false, false)]
	[InlineData(MidasRelicRarity.Ancient, false, false)]
	[InlineData(MidasRelicRarity.Event, false, false)]
	[InlineData(MidasRelicRarity.None, false, false)]
	[InlineData(MidasRelicRarity.Rare, true, false)]
	public void CanSellExcludesStartPackAndUnpricedRarities(MidasRelicRarity rarity, bool startPack, bool expected) =>
		Assert.Equal(expected, MidasRule.CanSell(rarity, startPack));

	[Fact]
	public void ResaleAlwaysCostsMoreThanTheSale()
	{
		foreach (int cost in new[] { 175, 200, 225, 275 })
			Assert.True(MidasRule.ResaleFloor(MidasRule.SalePrice(cost)) > MidasRule.SalePrice(cost));
	}

	[Theory]
	[InlineData(0, 1, 50)]
	[InlineData(2, 1, 60)]
	[InlineData(0, 2, 50)]
	[InlineData(2, 2, 70)]
	public void BankReopenFollowsTheMerchantRise(int boughtHere, int scale, int price) =>
		Assert.Equal(price, MidasRule.BankReopen(boughtHere, scale));

	[Fact]
	public void VintageRelicReplacesRemovedCardsInNormalFightsOnly()
	{
		Assert.Equal(1, MidasRule.VintageRelics(true, true, 1));
		Assert.Equal(0, MidasRule.VintageRelics(true, false, 1));
		Assert.Equal(0, MidasRule.VintageRelics(false, true, 1));
		Assert.Equal(0, MidasRule.VintageRelics(true, true, 0));
	}

	[Fact]
	public void LoanRepaymentLeavesTheBorrowerAboveTheMelt()
	{
		int repay = MidasRule.Repayable(10, 100, 25);
		Assert.InRange(repay, 1, 100);
		Assert.False(MidasRule.Ruined(10 + 100 - repay, 25));
		Assert.True(MidasRule.Ruined(10 + 100 - repay - 1, 25));
		Assert.Equal(0, MidasRule.Repayable(0, 10, 25));
		Assert.Equal(100, MidasRule.Repayable(10_000, 100, 25));
	}
}
