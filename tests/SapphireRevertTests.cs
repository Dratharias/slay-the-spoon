using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SapphireRevertTests
{
	private static PickupSnapshot S(int gold = 100, int hp = 70, int potions = 0, params (int, string)[] deck) =>
		new(deck.ToDictionary(c => c.Item1, c => c.Item2), gold, hp, potions);

	[Fact] public void Plan_UpgradedCardIsRestored()
	{
		var p = SapphireRevert.Plan(S(deck: [(1, "Strike+0"), (2, "Bash+0")]), S(deck: [(1, "Strike+1"), (2, "Bash+0")]));
		Assert.True(p.Reversible);
		Assert.Equal([1], p.Restore);
	}
	[Fact] public void Plan_AddedCardIsRemoved() => Assert.Equal([7], SapphireRevert.Plan(S(deck: [(1, "Strike")]), S(deck: [(1, "Strike"), (7, "Feed")])).Remove);
	[Fact] public void Plan_RemovedCardIsIrreversible() => Assert.False(SapphireRevert.Plan(S(deck: [(1, "Strike")]), S()).Reversible);
	[Fact] public void Plan_TransformedCardIsIrreversible() => Assert.False(SapphireRevert.Plan(S(deck: [(1, "Strike")]), S(deck: [(2, "Bash")])).Reversible);
	[Fact] public void Plan_GoldAndMaxHpGainsAreTakenBack()
	{
		var p = SapphireRevert.Plan(S(), S(gold: 400, hp: 80));
		Assert.Equal(300, p.GoldToLose);
		Assert.Equal(10, p.MaxHpToLose);
	}
	[Fact] public void Plan_NothingChangedIsAPlainSwap()
	{
		var p = SapphireRevert.Plan(S(), S());
		Assert.True(p.Reversible);
		Assert.Empty(p.Restore);
		Assert.Empty(p.Remove);
		Assert.Equal(0, p.GoldToLose);
	}
	[Fact] public void Plan_NewPotionsAreDiscarded() => Assert.Equal(2, SapphireRevert.Plan(S(potions: 1), S(potions: 3)).PotionsToDiscard);
	[Fact] public void Plan_LossesDoNotBecomeGains()
	{
		var p = SapphireRevert.Plan(S(potions: 3), S(gold: 50, hp: 60, potions: 1));
		Assert.Equal(0, p.GoldToLose);
		Assert.Equal(0, p.MaxHpToLose);
		Assert.Equal(0, p.PotionsToDiscard);
	}
}
