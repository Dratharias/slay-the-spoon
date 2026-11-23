using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class CurseRuleTests
{
	private static IReadOnlyDictionary<string, CurseRow> Table()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "mod", "SlaySpoon.json"))) dir = dir.Parent;
		return CurseRule.ParseTable(File.ReadAllText(Path.Combine(dir!.FullName, "data", "curses", "curses.json")));
	}

	[Fact]
	public void ParsesEveryRow()
	{
		var t = Table();
		Assert.Equal(22, t.Count);
		var b = t["BAD_LUCK"];
		Assert.Equal("HpLoss", b.Played);
		Assert.Equal(13, b.Amount);
		Assert.Equal(20, b.AmountPlus);
		Assert.Equal("HpLoss", b.SelfVar);
	}

	[Fact]
	public void GrimoireTwelveOnlyForUpgradedNoEffect()
	{
		var t = Table();
		Assert.Equal(12, CurseRule.GrimoireDamageFor(t["CLUMSY"], true));
		Assert.Equal(5, CurseRule.GrimoireDamageFor(t["CLUMSY"], false));
		Assert.Equal(5, CurseRule.GrimoireDamageFor(t["BAD_LUCK"], true));
		Assert.Equal(12, CurseRule.GrimoireDamageFor(null, true));
	}

	[Fact]
	public void NecronomicurseKeepsCostOne()
	{
		var t = Table();
		Assert.Equal(1, CurseRule.CostFor(t["STS1_NECRONOMICURSE"], -1, true));
		Assert.Equal(0, CurseRule.CostFor(t["BAD_LUCK"], -1, true));
		Assert.Equal(2, CurseRule.CostFor(t["ENTHRALLED"], 2, false));
		Assert.Equal(1, CurseRule.CostFor(t["BAD_LUCK"], -1, false));
	}

	[Fact]
	public void EnemyAmountFollowsUpgrade()
	{
		var b = Table()["BAD_LUCK"];
		Assert.Equal(13, CurseRule.EnemyAmount(b, false));
		Assert.Equal(20, CurseRule.EnemyAmount(b, true));
	}

	[Theory]
	[InlineData(true, true, true)]
	[InlineData(true, false, false)]
	[InlineData(false, true, false)]
	[InlineData(false, false, false)]
	public void PlayableOnlyWithModeAndGrimoire(bool mode, bool grim, bool expected) =>
		Assert.Equal(expected, CurseRule.IsPlayable(mode, grim));

	[Fact]
	public void TwoInThreeRelicRoll()
	{
		Assert.False(CurseRule.CursedRelicRoll(0));
		Assert.True(CurseRule.CursedRelicRoll(1));
		Assert.True(CurseRule.CursedRelicRoll(2));
		Assert.False(CurseRule.CursedRelicRoll(3));
	}

	[Fact]
	public void CursedRelicPickSkipsHeld()
	{
		var pool = new[] { "A", "B", "C" };
		Assert.Null(CurseRule.PickCursedRelic(pool, new HashSet<string> { "A", "B", "C" }, 7));
		Assert.Equal("B", CurseRule.PickCursedRelic(pool, new HashSet<string> { "A", "C" }, 7));
		Assert.Equal("C", CurseRule.PickCursedRelic(pool, new HashSet<string> { "A" }, 3));
	}

	[Fact]
	public void EventRewardThirdsAndFold()
	{
		Assert.Equal(CurseEventReward.CursedRelic, CurseRule.EventReward(0, true));
		Assert.Equal(CurseEventReward.Gold, CurseRule.EventReward(1, true));
		Assert.Equal(CurseEventReward.Heal, CurseRule.EventReward(2, true));
		Assert.Equal(CurseEventReward.Gold, CurseRule.EventReward(0, false));
	}

	[Fact]
	public void NormalityAndGuiltyNerfs()
	{
		var t = Table();
		Assert.Equal(new[] { 3, 4 }, t["NORMALITY"].Self);
		Assert.Equal(new[] { 5, 3 }, t["GUILTY"].Self);
	}

	[Fact]
	public void PlayCostFollowsVanillaPlayability()
	{
		var t = Table();
		Assert.Equal(1, CurseRule.PlayedCost(t["BAD_LUCK"], -1, false, -1));
		Assert.Equal(0, CurseRule.PlayedCost(t["BAD_LUCK"], -1, true, -1));
		Assert.Equal(1, CurseRule.PlayedCost(t["STS1_NECRONOMICURSE"], -1, true, -1));
		Assert.Null(CurseRule.PlayedCost(t["ENTHRALLED"], 2, false, 2));
		Assert.Equal(0, CurseRule.PlayedCost(t["ENTHRALLED"], 2, true, 2));
		Assert.Equal(0, CurseRule.PlayedCost(t["STS1_PRIDE"], 1, true, 1));
		Assert.Equal(1, CurseRule.PlayedCost(null, -1, false, -1));
	}

	[Fact]
	public void PlayedAmountPerKind()
	{
		var t = Table();
		Assert.Equal(13, CurseRule.PlayedAmount(t["BAD_LUCK"], false, 7, 4));
		Assert.Equal(20, CurseRule.PlayedAmount(t["BAD_LUCK"], true, 7, 4));
		Assert.Equal(4, CurseRule.PlayedAmount(t["REGRET"], false, 4, 9));
		Assert.Equal(8, CurseRule.PlayedAmount(t["REGRET"], true, 4, 9));
		Assert.Equal(0, CurseRule.PlayedAmount(t["REGRET"], false, 0, 9));
		Assert.Equal(3, CurseRule.PlayedAmount(t["STS1_PAIN"], false, 9, 3));
		Assert.Equal(6, CurseRule.PlayedAmount(t["STS1_PAIN"], true, 9, 3));
		Assert.Equal(0, CurseRule.PlayedAmount(t["CLUMSY"], true, 5, 5));
		Assert.Equal(15, CurseRule.PlayedAmount(t["DEBT"], true, 0, 0));
		Assert.Equal(5, CurseRule.PlayedAmount(t["STS1_PARASITE"], true, 0, 0));
	}

	[Fact]
	public void EmbraceGoesBeforeLeave()
	{
		Assert.Equal(1, CurseRule.EmbraceIndex(new[] { "A.options.TAKE", "A.options.LEAVE" }));
		Assert.Equal(1, CurseRule.EmbraceIndex(new[] { "A.options.TAKE", "A.options.PROCEED", "A.options.X" }));
		Assert.Equal(2, CurseRule.EmbraceIndex(new[] { "A.options.TAKE", "A.options.TRADE" }));
		Assert.Equal(0, CurseRule.EmbraceIndex(Array.Empty<string>()));
	}

	[Fact]
	public void EventHealIsAQuarterAtLeastOne()
	{
		Assert.Equal(20, CurseRule.EventHeal(80));
		Assert.Equal(18, CurseRule.EventHeal(75));
		Assert.Equal(1, CurseRule.EventHeal(3));
	}

	[Fact]
	public void CursedRelicNumbers()
	{
		Assert.Equal(0, CurseRule.PinDollStrength(2));
		Assert.Equal(1, CurseRule.PinDollStrength(3));
		Assert.Equal(2, CurseRule.PinDollStrength(7));
		Assert.Equal(0, CurseRule.PinDollStrength(-1));
		Assert.Equal(0, CurseRule.ReliquaryGold(0));
		Assert.Equal(15, CurseRule.ReliquaryGold(5));
		Assert.Equal(4, CurseRule.BloodPactBlock);
	}

	[Fact]
	public void RelicStreamDiffersPerBagKey()
	{
		Assert.NotEqual(CurseRule.RelicStream(5, 0, 0), CurseRule.RelicStream(5, 1, 0));
		Assert.NotEqual(CurseRule.RelicStream(5, 0, 0), CurseRule.RelicStream(5, -1, 0));
		Assert.NotEqual(CurseRule.RelicStream(5, 0, 0), CurseRule.RelicStream(5, 0, 1));
	}

	[Fact]
	public void OnlyCursesWithAPlayedEffectNeedATarget()
	{
		Assert.True(CurseRule.NeedsTarget(new CurseRow("BAD_LUCK", "HpLoss", 13, 20, "HpLoss", null, 0)));
		Assert.False(CurseRule.NeedsTarget(new CurseRow("CLUMSY", "None", 0, 0, null, null, 0)));
		Assert.False(CurseRule.NeedsTarget(null));
	}
}
