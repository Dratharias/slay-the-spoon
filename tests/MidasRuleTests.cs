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
	[InlineData(3900, 390)]
	[InlineData(300, 30)]
	[InlineData(250, 25)]
	[InlineData(100, 25)]
	[InlineData(20, 20)]
	[InlineData(0, 0)]
	public void MeltIsTenPercentAtLeastTwentyFive(int gold, int melt) => Assert.Equal(melt, MidasRule.Melt(gold));

	[Theory]
	[InlineData(0, true)]
	[InlineData(25, true)]
	[InlineData(26, false)]
	[InlineData(3900, false)]
	public void RuinedWhenTheMeltEmptiesThePurse(int gold, bool ruined) => Assert.Equal(ruined, MidasRule.Ruined(gold));

	[Theory]
	[InlineData(100, 74, true)]
	[InlineData(100, 75, false)]
	[InlineData(100, 101, false)]
	[InlineData(3900, 3000, true)]
	public void SpendingMustLeaveEnoughToSurvive(int gold, int price, bool ok) => Assert.Equal(ok, MidasRule.CanSpend(gold, price));

	[Fact]
	public void PricesRiseTenPercentPerPurchase()
	{
		Assert.Equal(1m, MidasRule.PriceFactor(0));
		Assert.Equal(1.1m, MidasRule.PriceFactor(1));
		Assert.Equal(1.5m, MidasRule.PriceFactor(5));
	}
}
