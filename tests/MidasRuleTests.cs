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
}
