using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SouvenirRuleTests
{
	[Fact]
	public void NoOwner_KeepsRunMode()
	{
		Assert.Equal(CardSetMode.Sts2Only, SouvenirRule.EffectiveMode(CardSetMode.Sts2Only, ownsSouvenir: false));
		Assert.Equal(CardSetMode.Sts1Only, SouvenirRule.EffectiveMode(CardSetMode.Sts1Only, ownsSouvenir: false));
		Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Hybrid, ownsSouvenir: false));
	}

	[Fact]
	public void Owner_GoesHybrid()
	{
		Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Sts2Only, ownsSouvenir: true));
		Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Sts1Only, ownsSouvenir: true));
	}

	[Fact]
	public void HybridRun_StaysHybrid()
	{
		Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Hybrid, ownsSouvenir: true));
	}

	[Fact]
	public void Offered_OutsideSouvenirAndDailyRuns()
	{
		Assert.True(SouvenirRule.IsOffered(souvenirRun: false, isDaily: false));
		Assert.False(SouvenirRule.IsOffered(souvenirRun: true, isDaily: false));
		Assert.False(SouvenirRule.IsOffered(souvenirRun: false, isDaily: true));
	}
}
