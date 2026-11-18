using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ForcedTradeRuleTests
{
	[Fact]
	public void SoloHasNoReceiver() => Assert.Equal(-1, ForcedTradeRule.Receiver(0, 1, 0));

	[Theory]
	[InlineData(0, 2, 0, 1)]
	[InlineData(1, 2, 0, 0)]
	[InlineData(1, 4, 0, 0)]
	[InlineData(1, 4, 1, 2)]
	[InlineData(1, 4, 2, 3)]
	[InlineData(3, 4, 2, 2)]
	public void ReceiverIsNeverTheGiver(int giver, int players, int roll, int receiver) =>
		Assert.Equal(receiver, ForcedTradeRule.Receiver(giver, players, roll));

	[Fact]
	public void EveryRollGivesAnotherPlayer()
	{
		for (int players = 2; players <= 4; players++)
		for (int giver = 0; giver < players; giver++)
		{
			var seen = Enumerable.Range(0, players - 1).Select(r => ForcedTradeRule.Receiver(giver, players, r)).ToHashSet();
			Assert.DoesNotContain(giver, seen);
			Assert.Equal(players - 1, seen.Count);
		}
	}
}
