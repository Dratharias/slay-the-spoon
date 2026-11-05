namespace SlaySpoon.Core;

/// <summary>Which game a card offered in a mixed pool belongs to; a Hybrid duplicate slot holds both versions.</summary>
public enum OfferSide
{
	Neutral,
	Sts1,
	Sts2,
}

/// <summary>
/// Offer weights for one draw among same-rarity cards (owner decisions 2026-11-05):
/// <list type="bullet">
/// <item>Souvenir lean: a per-run lean rolled on the seed gives the favored game's unique cards 60 % of the share the
/// two games' unique cards hold together (duplicate slots keep their count share); rarity odds are unchanged.</item>
/// <item>Cross-game combos: a card that would complete a listed pair the team already holds (any player's deck) has
/// its weight multiplied by 0.5. Pairs come from <c>docs/review/hybrid-combos.md</c>.</item>
/// </list>
/// Equal weights mean a vanilla draw, so runs where neither rule applies keep vanilla seeds.
/// </summary>
public static class OfferWeights
{
	public const double LeanShare = 0.6;
	public const double ComboFactor = 0.5;

	/// <summary>Owner-kept pairs (2026-11-05), by card id; a pair counts team-wide and either card may come second.</summary>
	public static readonly IReadOnlyList<(string A, string B)> ComboPairs = new[]
	{
		("STS1_TERROR", "DOMINATE"),
		("STS1_TERROR", "BULLY"),
		("STS1_CHOKE", "STS1_FLURRY_OF_BLOWS"),
	};

	/// <summary>The cards that complete a pair with one of <paramref name="teamHeld"/>.</summary>
	public static HashSet<string> ComboCompleters(IEnumerable<string> teamHeld)
	{
		var held = teamHeld as ISet<string> ?? teamHeld.ToHashSet();
		var result = new HashSet<string>();
		foreach (var (a, b) in ComboPairs)
		{
			if (held.Contains(a)) result.Add(b);
			if (held.Contains(b)) result.Add(a);
		}
		return result;
	}

	/// <summary>
	/// One weight per card. <paramref name="lean"/> is the favored game (null: no lean, for a player without the
	/// Souvenir); <paramref name="completers"/> comes from <see cref="ComboCompleters"/>.
	/// </summary>
	public static double[] Compute(IReadOnlyList<(string Id, OfferSide Side)> cards, OfferSide? lean, ISet<string> completers)
	{
		var weights = new double[cards.Count];
		int sts1 = cards.Count(c => c.Side == OfferSide.Sts1);
		int sts2 = cards.Count(c => c.Side == OfferSide.Sts2);
		bool leaning = (lean is OfferSide.Sts1 or OfferSide.Sts2) && sts1 > 0 && sts2 > 0;
		for (int i = 0; i < cards.Count; i++)
		{
			double weight = 1;
			if (leaning && cards[i].Side != OfferSide.Neutral)
			{
				int total = sts1 + sts2;
				int count = cards[i].Side == OfferSide.Sts1 ? sts1 : sts2;
				double share = cards[i].Side == lean ? LeanShare : 1 - LeanShare;
				weight = share * total / count;
			}
			if (completers.Contains(cards[i].Id)) weight *= ComboFactor;
			weights[i] = weight;
		}
		return weights;
	}

	/// <summary>True when every weight is the same, so the vanilla uniform draw applies.</summary>
	public static bool IsUniform(IReadOnlyList<double> weights) =>
		weights.Count == 0 || weights.All(w => Math.Abs(w - weights[0]) < 1e-9);

	/// <summary>The index <paramref name="roll"/> (in [0, 1)) lands on.</summary>
	public static int Pick(IReadOnlyList<double> weights, double roll)
	{
		double target = roll * weights.Sum();
		for (int i = 0; i < weights.Count; i++)
		{
			target -= weights[i];
			if (target < 0) return i;
		}
		return weights.Count - 1;
	}
}
