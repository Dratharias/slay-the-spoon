using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherRunRuleTests
{
	[Theory]
	[InlineData(true, 1, true, CardSetMode.Sts1Only)]
	[InlineData(true, 2, true, CardSetMode.Sts2Only)]
	[InlineData(true, 1, false, CardSetMode.Sts2Only)]
	[InlineData(false, 1, true, CardSetMode.Sts2Only)]
	[InlineData(true, 0, false, CardSetMode.Sts2Only)]
	public void ImplicitModeRequiresEligibleSoloWatcher(bool eligibleMode, int players, bool watcher, CardSetMode expected)
	{
		Assert.Equal(expected, WatcherRunRule.EffectiveMode(CardSetMode.Sts2Only, eligibleMode, players, watcher));
	}

	[Theory]
	[InlineData(CardSetMode.Sts1Only)]
	[InlineData(CardSetMode.Hybrid)]
	public void ExplicitModesArePreserved(CardSetMode configured)
	{
		Assert.Equal(configured, WatcherRunRule.EffectiveMode(configured, true, 1, true));
		Assert.Equal(configured, WatcherRunRule.EffectiveMode(configured, true, 2, true));
	}

	[Fact]
	public void SouvenirStillOverridesImplicitSoloMode()
	{
		CardSetMode mode = WatcherRunRule.EffectiveMode(CardSetMode.Sts2Only, true, 1, true);
		Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(mode, ownsSouvenir: true));
		Assert.Equal(CardSetMode.Sts1Only, SouvenirRule.EffectiveMode(mode, ownsSouvenir: false));
	}
}

public class WatcherRandomRuleTests
{
	[Theory]
	[InlineData(true, true, true)]
	[InlineData(false, true, false)]
	[InlineData(true, false, false)]
	[InlineData(false, false, false)]
	public void RandomDrawsHerOnlyInSoloStandard(bool singleplayer, bool standard, bool expected)
	{
		Assert.Equal(expected, WatcherRunRule.RandomIncludesWatcher(singleplayer, standard));
	}
}
