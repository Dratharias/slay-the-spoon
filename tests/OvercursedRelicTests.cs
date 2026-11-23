using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class OvercursedRelicTests
{
	[Theory]
	[InlineData(CursedRelicSource.Chest, true, false, true)]
	[InlineData(CursedRelicSource.Chest, false, true, false)]
	[InlineData(CursedRelicSource.Elite, false, true, false)]
	[InlineData(CursedRelicSource.Merchant, false, true, true)]
	[InlineData(CursedRelicSource.Merchant, true, false, true)]
	[InlineData(CursedRelicSource.Event, true, false, false)]
	[InlineData(CursedRelicSource.Event, false, true, true)]
	[InlineData(CursedRelicSource.Event, false, false, false)]
	public void SourcesFollowTheModes(CursedRelicSource source, bool cursedDeck, bool overcursed, bool expected) =>
		Assert.Equal(expected, OvercursedRule.CursedSource(source, cursedDeck, overcursed));

	[Fact]
	public void OvercursedRollIsFiveInSix()
	{
		Assert.Equal(5, Enumerable.Range(0, 6).Count(r => OvercursedRule.CursedRelicRoll(r, true)));
		Assert.Equal(4, Enumerable.Range(0, 6).Count(r => OvercursedRule.CursedRelicRoll(r, false)));
	}

	[Fact]
	public void EmbraceLeansToCursedRelics()
	{
		Assert.Equal(2, Enumerable.Range(0, 3).Count(r => OvercursedRule.EmbraceDraw(r, true, true) == CurseEventReward.CursedRelic));
		Assert.Equal(1, Enumerable.Range(0, 3).Count(r => OvercursedRule.EmbraceDraw(r, true, false) == CurseEventReward.CursedRelic));
		Assert.DoesNotContain(CurseEventReward.CursedRelic, Enumerable.Range(0, 3).Select(r => OvercursedRule.EmbraceDraw(r, false, true)));
	}
}
