using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class VersionPairsTests
{
	[Fact]
	public void VersionPairs_MapBothWays()
	{
		Sts1Entry[] entries =
		{
			new("STS1_CLEAVE", null, Sts1Relation.Unique),
			new("STS1_RUPTURE", "RUPTURE", Sts1Relation.Duplicate),
			new("STS1_RAMPAGE", "RAMPAGE", Sts1Relation.SameKeepSts1),
		};
		IReadOnlyDictionary<string, string> pairs = VersionPairs.Of(entries);
		Assert.Equal(2, pairs.Count);
		Assert.Equal("STS1_RUPTURE", pairs["RUPTURE"]);
		Assert.Equal("RUPTURE", pairs["STS1_RUPTURE"]);
	}
}
