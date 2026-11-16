using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonRuleTests
{
	[Fact]
	public void ShouldInsertWhenEverythingLines_Up() =>
		Assert.True(SpoonRule.ShouldInsert(true, true, true, [310, 80], false));

	[Fact]
	public void ShouldNotInsertOtherwise()
	{
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [299, 250], false));
		Assert.False(SpoonRule.ShouldInsert(true, true, false, [310], false));
		Assert.False(SpoonRule.ShouldInsert(false, true, true, [310], false));
		Assert.False(SpoonRule.ShouldInsert(true, true, true, [310], true));
		Assert.False(SpoonRule.ShouldInsert(true, false, true, [310], false));
	}

	[Fact]
	public void InsertIndexByActShape()
	{
		Assert.Equal(3, SpoonRule.InsertIndex(3, false));
		Assert.Equal(3, SpoonRule.InsertIndex(4, true));
		Assert.Equal(-1, SpoonRule.InsertIndex(4, false));
		Assert.Equal(-1, SpoonRule.InsertIndex(5, true));
	}

	[Fact]
	public void FightersAreThoseAtThreshold() =>
		Assert.Equal([0, 2], SpoonRule.Fighters([300, 299, 450]));

	[Theory]
	[InlineData(50, 1, 300, 50)]
	[InlineData(50, 1, 600, 25)]
	[InlineData(50, 1, 400, 37)]
	[InlineData(50, 2, 600, 50)]
	[InlineData(30, 1, 100000, 1)]
	[InlineData(0, 1, 300, 0)]
	[InlineData(50, 1, 0, 50)]
	[InlineData(50, 0, 600, 50)]
	[InlineData(30, 1, 900, 10)]
	[InlineData(60, 1, 900, 20)]
	public void ScaledDamage(int b, int p, int hp, int expected) => Assert.Equal(expected, SpoonRule.ScaledDamage(b, p, hp));

	[Fact]
	public void DamageMultiplier() => Assert.Equal(0.5m, SpoonRule.DamageMultiplier(1, 600));

	[Theory]
	[InlineData(1, 1, 1500)]
	[InlineData(1, 2, 750)]
	[InlineData(2, 2, 1500)]
	[InlineData(1, 3, 500)]
	public void BossHp(int f, int p, int expected) => Assert.Equal(expected, SpoonRule.BossHp(f, p));

	[Fact]
	public void MoveCycles() =>
		Assert.Equal(
			[SpoonRule.SpoonMove.Bouchee, SpoonRule.SpoonMove.Bouchee, SpoonRule.SpoonMove.Ladle,
			 SpoonRule.SpoonMove.Bouchee, SpoonRule.SpoonMove.Bouchee, SpoonRule.SpoonMove.Ladle],
			Enumerable.Range(0, 6).Select(SpoonRule.Move).ToList());

	[Fact]
	public void ExtraRelicsTable()
	{
		Assert.Equal(7, SpoonRule.ExtraRelics.Select(r => r.Entry).Distinct().Count());
		Assert.All(SpoonRule.ExtraRelics, r => Assert.Contains(r.Deque, new[] { "Common", "Uncommon", "Rare" }));
	}

	[Theory]
	[InlineData(1, false)]
	[InlineData(5, false)]
	[InlineData(6, true)]
	[InlineData(300, true)]
	public void CanGiftNeedsMoreMaxHpThanTheGift(int ownerMaxHp, bool expected) =>
		Assert.Equal(expected, SpoonRule.CanGift(ownerMaxHp));

	[Theory]
	[InlineData(300, 295, 5)]
	[InlineData(6, 1, 5)]
	[InlineData(3, 1, 2)]
	[InlineData(10, 10, 0)]
	[InlineData(10, 12, 0)]
	[InlineData(20, 5, 5)]
	public void GiftTransfersWhatTheGiverLost(int before, int after, int expected) =>
		Assert.Equal(expected, SpoonRule.GiftTransferred(before, after));

	[Fact]
	public void JuiceSlot()
	{
		Assert.Equal(-1, SpoonRule.JuiceSlot(3, true, 5));
		Assert.Equal(-1, SpoonRule.JuiceSlot(0, false, 5));
		Assert.Equal(2, SpoonRule.JuiceSlot(3, false, 5));
		Assert.Equal(1, SpoonRule.JuiceSlot(3, false, -5));
	}

	[Fact]
	public void DamageFactorTruncatesToScaledDamage()
	{
		for (int players = 1; players <= 4; players++)
			for (int team = 1; team <= 2400 * players; team += players == 1 ? 1 : 7)
				for (int amount = 1; amount <= 160; amount += 3)
				{
					decimal factor = SpoonRule.DamageFactor(amount, players, team);
					Assert.Equal(SpoonRule.ScaledDamage(amount, players, team), (int)(amount * factor));
				}
	}

	[Fact]
	public void DamageFactorLeavesNothingAndInvalidTeamsAlone()
	{
		Assert.Equal(1m, SpoonRule.DamageFactor(0m, 1, 300));
		Assert.Equal(30, (int)(30m * SpoonRule.DamageFactor(30m, 2, 0)));
	}

	[Fact]
	public void FightersAtFightFallsBackToEveryone()
	{
		Assert.Equal([1], SpoonRule.FightersAtFight([120, 300, 299]));
		Assert.Equal([0, 1], SpoonRule.FightersAtFight([120, 80]));
	}
}
