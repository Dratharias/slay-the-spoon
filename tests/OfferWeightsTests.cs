using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class OfferWeightsTests
{
	private static readonly HashSet<string> None = new();

	[Fact]
	public void NoLeanNoComboIsUniform()
	{
		var w = OfferWeights.Compute(new[] { ("A", OfferSide.Sts1), ("B", OfferSide.Sts2), ("C", OfferSide.Neutral) }, null, None);
		Assert.True(OfferWeights.IsUniform(w));
	}

	[Theory]
	[InlineData(OfferSide.Sts1, 0.6)]
	[InlineData(OfferSide.Sts2, 0.4)]
	public void LeanGivesTheFavoredGameSixtyPercentOfTheUniqueShare(OfferSide lean, double sts1Share)
	{
		var cards = new[] { ("A", OfferSide.Sts1), ("B", OfferSide.Sts2), ("C", OfferSide.Sts2), ("D", OfferSide.Sts2), ("N", OfferSide.Neutral) };
		var w = OfferWeights.Compute(cards, lean, None);
		double total = w.Sum();
		Assert.Equal(sts1Share * 4 / 5, w[0] / total, 6);
		Assert.Equal(1.0 / 5, w[4] / total, 6);
	}

	[Fact]
	public void LeanNeedsBothGames()
	{
		var w = OfferWeights.Compute(new[] { ("A", OfferSide.Sts2), ("B", OfferSide.Sts2), ("N", OfferSide.Neutral) }, OfferSide.Sts1, None);
		Assert.True(OfferWeights.IsUniform(w));
	}

	[Fact]
	public void CompletersAreHalved()
	{
		var completers = OfferWeights.ComboCompleters(new[] { "STS1_TERROR", "STRIKE" });
		Assert.Equal(new HashSet<string> { "DOMINATE", "BULLY" }, completers);
		var w = OfferWeights.Compute(new[] { ("DOMINATE", OfferSide.Sts2), ("ANGER", OfferSide.Sts2) }, null, completers);
		Assert.Equal(new[] { 0.5, 1.0 }, w);
	}

	[Fact]
	public void PairsWorkBothWays() =>
		Assert.Contains("STS1_CHOKE", OfferWeights.ComboCompleters(new[] { "STS1_FLURRY_OF_BLOWS" }));

	[Theory]
	[InlineData(0.0, 0)]
	[InlineData(0.24, 0)]
	[InlineData(0.26, 1)]
	[InlineData(0.99, 1)]
	public void PickFollowsTheWeights(double roll, int expected) =>
		Assert.Equal(expected, OfferWeights.Pick(new[] { 1.0, 3.0 }, roll));
}
