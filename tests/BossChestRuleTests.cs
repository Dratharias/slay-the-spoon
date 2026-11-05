using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class BossChestRuleTests
{
	private static List<int> Keep(List<int> left) => left;

	private static int Lowest(IReadOnlyList<int> voters, int relic) => voters.Min();

	[Theory]
	[InlineData(0, false, false)]
	[InlineData(1, false, true)]
	[InlineData(2, false, true)]
	[InlineData(3, false, false)]
	[InlineData(1, true, false)]
	public void OnlyTheAct2And3AncientsOffer(int act, bool neow, bool offers) =>
		Assert.Equal(offers, BossChestRule.Offers(act, neow));

	[Theory]
	[InlineData("PANDORAS_BOX", 1, false, true)]
	[InlineData("PANDORAS_BOX", 1, true, false)]
	[InlineData("ECTOPLASM", 1, false, true)]
	[InlineData("SOZU", 2, false, false)]
	[InlineData("PHILOSOPHERS_STONE", 2, false, true)]
	[InlineData("VELVET_CHOKER", 1, false, false)]
	[InlineData("ASTROLABE", 2, true, true)]
	public void FollowsDarvRules(string relic, int act, bool clearsDeck, bool allowed) =>
		Assert.Equal(allowed, BossChestRule.NativeAllowed(relic, act, clearsDeck));

	[Fact]
	public void CharacterRelicsNeedEveryTakerOnTheCharacter()
	{
		Assert.True(BossChestRule.CharacterRelicsShared(new[] { "IRONCLAD" }, "IRONCLAD"));
		Assert.True(BossChestRule.CharacterRelicsShared(new[] { "SILENT", "SILENT" }, "SILENT"));
		Assert.False(BossChestRule.CharacterRelicsShared(new[] { "SILENT", "DEFECT" }, "SILENT"));
		Assert.False(BossChestRule.CharacterRelicsShared(Array.Empty<string>(), "SILENT"));
	}

	[Fact]
	public void SoloTakesTheVotedRelic()
	{
		var awards = BossChestRule.Resolve(new Dictionary<int, int?> { [0] = 2 }, 3, Lowest, Keep);
		Assert.Equal(new[] { new BossChestAward(0, 2, BossChestOutcome.Only) }, awards);
	}

	[Fact]
	public void DistinctVotesNeedNoFight()
	{
		var awards = BossChestRule.Resolve(new Dictionary<int, int?> { [0] = 1, [1] = 0 }, 3,
			(_, _) => throw new InvalidOperationException("no fight"), Keep);
		Assert.Equal(new BossChestAward(0, 1, BossChestOutcome.Only), awards[0]);
		Assert.Equal(new BossChestAward(1, 0, BossChestOutcome.Only), awards[1]);
	}

	[Fact]
	public void LosersGetTheUnvotedRelicsInShuffledOrder()
	{
		var votes = new Dictionary<int, int?> { [0] = 0, [1] = 0, [2] = 0 };
		var awards = BossChestRule.Resolve(votes, 3, (voters, _) => voters[1], left => left.AsEnumerable().Reverse().ToList());
		Assert.Equal(new BossChestAward(0, 2, BossChestOutcome.Consolation), awards[0]);
		Assert.Equal(new BossChestAward(1, 0, BossChestOutcome.WonFight), awards[1]);
		Assert.Equal(new BossChestAward(2, 1, BossChestOutcome.Consolation), awards[2]);
	}

	[Fact]
	public void ALoserBeyondTheUnvotedRelicsGetsNothing()
	{
		var votes = new Dictionary<int, int?> { [0] = 0, [1] = 0, [2] = 1, [3] = 1 };
		var awards = BossChestRule.Resolve(votes, 3, Lowest, Keep);
		Assert.Equal(new BossChestAward(1, 2, BossChestOutcome.Consolation), awards[1]);
		Assert.Equal(new BossChestAward(3, null, BossChestOutcome.Empty), awards[3]);
	}

	[Fact]
	public void ASkipperGetsNoConsolation()
	{
		var votes = new Dictionary<int, int?> { [0] = null, [1] = 1 };
		var awards = BossChestRule.Resolve(votes, 3, Lowest, Keep);
		Assert.Equal(new BossChestAward(0, null, BossChestOutcome.Skipped), awards[0]);
		Assert.Equal(new BossChestAward(1, 1, BossChestOutcome.Only), awards[1]);
	}

	[Fact]
	public void RejectsAnOutOfRangeVote() =>
		Assert.Throws<ArgumentException>(() => BossChestRule.Resolve(new Dictionary<int, int?> { [0] = 3 }, 3, Lowest, Keep));
}
