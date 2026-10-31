using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SapphireRevertTests
{
	private static PickupSnapshot S(int gold = 100, int hp = 70, int potions = 0, int cur = 70, int slots = 3,
		params (int, string)[] deck) =>
		new(deck.ToDictionary(c => c.Item1, c => c.Item2), gold, hp, potions, cur, slots);

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

	[Fact] public void Plan_MaxHpGainAndItsHealAreTakenBack()
	{
		// Mango on a damaged player: max HP 70 -> 84, current HP 40 -> 54.
		var p = SapphireRevert.Plan(S(hp: 70, cur: 40), S(hp: 84, cur: 54));
		Assert.Equal(14, p.MaxHpToLose);
		Assert.Equal(14, p.HpToLose);
	}
	[Fact] public void Plan_HpLossNeverDropsBelowOne() => Assert.Equal(4, SapphireRevert.Plan(S(cur: 0), S(cur: 5)).HpToLose);
	[Fact] public void HpLoss_SkipsWhatTheMaxHpClampAlreadyTook() => Assert.Equal(0, SapphireRevert.HpLoss(14, alreadyLost: 14, currentHp: 70));
	[Fact] public void HpLoss_TakesTheRestAfterAPartialClamp() => Assert.Equal(6, SapphireRevert.HpLoss(14, alreadyLost: 8, currentHp: 70));
	[Fact] public void HpLoss_ClampsAtOneHp() => Assert.Equal(4, SapphireRevert.HpLoss(14, alreadyLost: 0, currentHp: 5));
	[Fact] public void HpLoss_NeverHeals() => Assert.Equal(0, SapphireRevert.HpLoss(3, alreadyLost: 0, currentHp: 1));
	[Fact] public void Plan_PotionSlotsAreTakenBack() => Assert.Equal(2, SapphireRevert.Plan(S(slots: 3), S(slots: 5)).PotionSlotsToLose);
	[Fact] public void Plan_HpAndSlotLossesDoNotBecomeGains()
	{
		var p = SapphireRevert.Plan(S(cur: 60, slots: 3), S(cur: 50, slots: 2));
		Assert.Equal(0, p.HpToLose);
		Assert.Equal(0, p.PotionSlotsToLose);
	}

	private static readonly PickupRevertPlan Reversible = SapphireRevert.Plan(S(deck: [(1, "Strike+0"), (2, "Bash")]),
		S(deck: [(1, "Strike+1"), (2, "Bash"), (3, "Feed")]));
	[Fact] public void SwapPlan_AllChecksPassReturnsThePlan() =>
		Assert.Same(Reversible, SapphireRevert.SwapPlan(Reversible, holdsRelic: true, teamHasSapphire: false, _ => true));
	[Fact] public void SwapPlan_NoRecordIsRefused() =>
		Assert.Null(SapphireRevert.SwapPlan(null, holdsRelic: true, teamHasSapphire: false, _ => true));
	[Fact] public void SwapPlan_RelicNoLongerHeldIsRefused() =>
		Assert.Null(SapphireRevert.SwapPlan(Reversible, holdsRelic: false, teamHasSapphire: false, _ => true));
	[Fact] public void SwapPlan_TeamWithSapphireIsRefused() =>
		Assert.Null(SapphireRevert.SwapPlan(Reversible, holdsRelic: true, teamHasSapphire: true, _ => true));
	[Fact] public void SwapPlan_IrreversiblePlanIsRefused() =>
		Assert.Null(SapphireRevert.SwapPlan(SapphireRevert.Plan(S(deck: [(1, "Strike")]), S()), holdsRelic: true, teamHasSapphire: false, _ => true));
	[Fact] public void SwapPlan_PlannedCardMissingFromDeckIsRefused()
	{
		Assert.Null(SapphireRevert.SwapPlan(Reversible, holdsRelic: true, teamHasSapphire: false, h => h != 1));
		Assert.Null(SapphireRevert.SwapPlan(Reversible, holdsRelic: true, teamHasSapphire: false, h => h != 3));
	}
	[Fact] public void SwapPlan_UntouchedCardsNeedNotBeInDeck() =>
		Assert.NotNull(SapphireRevert.SwapPlan(Reversible, holdsRelic: true, teamHasSapphire: false, h => h != 2));
}

public class PickupLedgerTests
{
	[Fact] public void Find_ReturnsTheEntryForItsFloor()
	{
		var ledger = new PickupLedger<string, int>();
		ledger.Record("a:Mango", floor: 9, entry: 1);
		Assert.True(ledger.TryFind("a:Mango", floor: 9, out int entry));
		Assert.Equal(1, entry);
	}
	[Fact] public void Find_OtherFloorIsMissing()
	{
		var ledger = new PickupLedger<string, int>();
		ledger.Record("a:Mango", floor: 9, entry: 1);
		Assert.False(ledger.TryFind("a:Mango", floor: 10, out _));
		Assert.False(ledger.TryFind("b:Mango", floor: 9, out _));
	}
	[Fact] public void Clear_ForgetsEverything()
	{
		var ledger = new PickupLedger<string, int>();
		ledger.Record("a:Mango", floor: 9, entry: 1);
		ledger.Expect(["Mango"]);
		ledger.Clear();
		Assert.False(ledger.TryFind("a:Mango", floor: 9, out _));
		Assert.False(ledger.ConsumeExpected("Mango"));
	}
	[Fact] public void ConsumeExpected_MatchesEachAwardedRelicOnce()
	{
		var ledger = new PickupLedger<string, int>();
		ledger.Expect(["Mango", "Mango", "Pear"]);
		Assert.True(ledger.ConsumeExpected("Mango"));
		Assert.True(ledger.ConsumeExpected("Mango"));
		Assert.False(ledger.ConsumeExpected("Mango"));
		Assert.False(ledger.ConsumeExpected("Anchor"));
		Assert.True(ledger.ConsumeExpected("Pear"));
	}

	[Fact] public void IsExpected_IsTrueUntilTheRelicIsConsumed()
	{
		var ledger = new PickupLedger<string, int>();
		ledger.Expect(["Mango"]);
		Assert.True(ledger.IsExpected("Mango"));
		Assert.False(ledger.IsExpected("Pear"));
		ledger.ConsumeExpected("Mango");
		Assert.False(ledger.IsExpected("Mango"));
	}

	[Theory]
	[InlineData(true, false, false, true)]   // the reward animation has not handed the relic out yet
	[InlineData(false, true, false, true)]   // handed out, its effects are still running
	[InlineData(false, true, true, false)]   // done: decide now
	[InlineData(false, false, false, false)] // never awarded here: refuse at once
	public void ShouldWait_OnlyForAPickupThatIsStillComing(bool expected, bool recorded, bool planReady, bool wait) =>
		Assert.Equal(wait, SapphireRevert.ShouldWait(expected, recorded, planReady));
}
