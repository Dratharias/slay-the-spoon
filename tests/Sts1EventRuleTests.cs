using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class Sts1EventRuleTests
{
	[Theory]
	[InlineData(Sts1EventKind.Vampires)]
	[InlineData(Sts1EventKind.Augmenter)]
	[InlineData(Sts1EventKind.Nest)]
	[InlineData(Sts1EventKind.GoldenIdolEvent)]
	[InlineData(Sts1EventKind.ForgottenAltar)]
	[InlineData(Sts1EventKind.MoaiHead)]
	[InlineData(Sts1EventKind.CursedTome)]
	public void Sts1Run_AllowsEveryEvent_WithoutRelic(Sts1EventKind kind)
	{
		Assert.True(Sts1EventRule.IsAllowed(kind, sts1Run: true, players: 2, souvenirOwners: 0));
	}

	[Theory]
	[InlineData(Sts1EventKind.Vampires)]
	[InlineData(Sts1EventKind.Augmenter)]
	[InlineData(Sts1EventKind.Nest)]
	[InlineData(Sts1EventKind.GoldenIdolEvent)]
	[InlineData(Sts1EventKind.ForgottenAltar)]
	[InlineData(Sts1EventKind.MoaiHead)]
	[InlineData(Sts1EventKind.CursedTome)]
	public void NoRelicNoMode_AllowsNothing(Sts1EventKind kind)
	{
		Assert.False(Sts1EventRule.IsAllowed(kind, sts1Run: false, players: 1, souvenirOwners: 0));
	}

	[Theory]
	[InlineData(Sts1EventKind.Augmenter)]
	[InlineData(Sts1EventKind.Nest)]
	[InlineData(Sts1EventKind.GoldenIdolEvent)]
	[InlineData(Sts1EventKind.ForgottenAltar)]
	[InlineData(Sts1EventKind.MoaiHead)]
	[InlineData(Sts1EventKind.CursedTome)]
	public void OneOwner_AllowsAugmenterAndNest(Sts1EventKind kind)
	{
		Assert.True(Sts1EventRule.IsAllowed(kind, sts1Run: false, players: 3, souvenirOwners: 1));
	}

	[Fact]
	public void Vampires_NeedsEveryPlayerToOwnTheRelic()
	{
		Assert.False(Sts1EventRule.IsAllowed(Sts1EventKind.Vampires, sts1Run: false, players: 3, souvenirOwners: 2));
		Assert.True(Sts1EventRule.IsAllowed(Sts1EventKind.Vampires, sts1Run: false, players: 3, souvenirOwners: 3));
		Assert.True(Sts1EventRule.IsAllowed(Sts1EventKind.Vampires, sts1Run: false, players: 1, souvenirOwners: 1));
	}

	[Fact]
	public void CardOption_OpenInSts1RunOrForOwner()
	{
		Assert.True(Sts1EventRule.CardUnlocked(sts1Run: true, ownsSouvenir: false));
		Assert.True(Sts1EventRule.CardUnlocked(sts1Run: false, ownsSouvenir: true));
		Assert.False(Sts1EventRule.CardUnlocked(sts1Run: false, ownsSouvenir: false));
	}

	[Fact]
	public void Replace_OnlyInActTwo_WithCandidates_BelowChance()
	{
		Assert.True(Sts1EventRule.ShouldReplace(actIndex: 1, candidates: 2, roll: 0.1f));
		Assert.False(Sts1EventRule.ShouldReplace(actIndex: 0, candidates: 2, roll: 0.1f));
		Assert.False(Sts1EventRule.ShouldReplace(actIndex: 2, candidates: 2, roll: 0.1f));
		Assert.False(Sts1EventRule.ShouldReplace(actIndex: 1, candidates: 0, roll: 0.1f));
		Assert.False(Sts1EventRule.ShouldReplace(actIndex: 1, candidates: 2, roll: 0.5f));
	}

	[Theory]
	[InlineData(80, 24)]
	[InlineData(75, 23)]
	[InlineData(3, 1)]
	[InlineData(1, 0)]
	public void VampiresMaxHpLoss_IsThirtyPercentRoundedUp_LeavingOneHp(int maxHp, int loss)
	{
		Assert.Equal(loss, Sts1EventRule.VampiresMaxHpLoss(maxHp));
	}
}
