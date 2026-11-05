using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ColorlessCardRulesTests
{
	[Fact]
	public void ScrollPreservesCoopFilterWhenAvailable() => Assert.Equal(new[] { "coop" },
		ColorlessCardRules.PreferCoopCards(new[] { "ordinary", "coop" }, card => card == "coop"));

	[Fact]
	public void ScrollFallsBackToAllowedPoolWithoutAddingForeignCards() => Assert.Equal(new[] { "sts1" },
		ColorlessCardRules.PreferCoopCards(new[] { "sts1" }, _ => false));

	[Theory]
	[InlineData("STS1_APOTHEOSIS")]
	[InlineData("STS1_APPARITION")]
	[InlineData("STS1_BITE")]
	[InlineData("STS1_ENLIGHTENMENT")]
	[InlineData("STS1_JAX")]
	[InlineData("STS1_METAMORPHOSIS")]
	[InlineData("STS1_RITUAL_DAGGER")]
	public void SpecialAvailabilityDoesNotDependOnSourceRarity(string id) =>
		Assert.True(ColorlessCardRules.IsEventOnly(id));

	[Theory]
	[InlineData("STS1_DISCOVERY")]
	[InlineData("STS1_PANACHE")]
	[InlineData("STS1_FLASH_OF_STEEL")]
	[InlineData("STS1_CHILL")]
	public void OrdinaryCardsAreNotEventOnly(string id) => Assert.False(ColorlessCardRules.IsEventOnly(id));

	[Theory]
	[InlineData(CardSetMode.Sts2Only, Sts1Relation.Duplicate, false, false)]
	[InlineData(CardSetMode.Sts2Only, Sts1Relation.Duplicate, true, false)]
	[InlineData(CardSetMode.Sts2Only, Sts1Relation.SameKeepSts1, true, false)]
	[InlineData(CardSetMode.Sts1Only, Sts1Relation.Duplicate, false, true)]
	[InlineData(CardSetMode.Sts1Only, Sts1Relation.SameKeepSts1, false, true)]
	[InlineData(CardSetMode.Hybrid, Sts1Relation.SameKeepSts1, false, true)]
	[InlineData(CardSetMode.Hybrid, Sts1Relation.Duplicate, false, false)]
	[InlineData(CardSetMode.Hybrid, Sts1Relation.Duplicate, true, true)]
	[InlineData(CardSetMode.Sts1Only, Sts1Relation.Retired, true, false)]
	[InlineData(CardSetMode.Hybrid, Sts1Relation.Retired, true, false)]
	public void DirectGrantsRespectModeAndNamesakeRelation(CardSetMode mode, Sts1Relation relation,
		bool roll, bool expected) =>
		Assert.Equal(expected, ColorlessCardRules.UseSts1EventVersion(mode, relation, roll));
}
