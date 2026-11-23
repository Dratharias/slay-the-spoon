using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Curses.Forged;

/// <summary>What Bloody, Echo, Contagious and Ravenous do when a forged curse is played (from the Unholy Grimoire).</summary>
public static class CurseDefectEffects
{
	public static async Task Bloody(PlayerChoiceContext ctx, CardModel card)
	{
		int hp = ForgedCurseRule.BloodyHp(card.CurrentUpgradeLevel);
		if (hp <= 0) return;
		await CreatureCmd.Damage(ctx, card.Owner.Creature, hp, ValueProp.Unblockable | ValueProp.Unpowered, card, null);
	}

	/// <summary>Half the played effect, on the curse's owner (gold gained becomes gold lost).</summary>
	public static async Task Echo(PlayerChoiceContext ctx, CardModel card, string played, int amount)
	{
		if (amount <= 0) return;
		var owner = card.Owner;
		switch (played)
		{
			case "HpLoss":
			case "HpLossPerHandCard":
			case "HpLossPerCardPlayed":
				await CreatureCmd.Damage(ctx, owner.Creature, amount, ValueProp.Unblockable | ValueProp.Unpowered, card, null);
				break;
			case "Damage":
				await CreatureCmd.Damage(ctx, owner.Creature, amount, ValueProp.Unpowered, card, null);
				break;
			case "Weak":
				await PowerCmd.Apply<WeakPower>(ctx, owner.Creature, amount, null, card);
				break;
			case "Vulnerable":
				await PowerCmd.Apply<VulnerablePower>(ctx, owner.Creature, amount, null, card);
				break;
			case "GainGold":
				await PlayerCmd.LoseGold(Math.Min(amount, owner.Gold), owner);
				break;
			case "MaxHpLoss":
				await CreatureCmd.LoseMaxHp(ctx, owner.Creature, amount, isFromCard: true);
				break;
		}
	}

	private static Rng PlayRng(CardModel card)
	{
		var run = card.Owner.RunState;
		int round = card.CombatState?.RoundNumber ?? 0;
		int index = CombatManager.Instance.History.CardPlaysStarted.Count();
		return new Rng(run.Rng.Seed, ForgedCurseRule.PlayStream(run.TotalFloor, round, index));
	}

	/// <summary>A random Cursed Deck curse (+0) shuffled into the owner's draw pile.</summary>
	public static async Task Contagious(CardModel card)
	{
		if (card.CombatState is not { } combat) return;
		var pool = CursedDeckCurses.Pool(card.Owner);
		if (pool.Count == 0) return;
		var canonical = pool[PlayRng(card).NextInt(pool.Count)];
		var curse = combat.CreateCard(canonical, card.Owner);
		await CardPileCmd.AddGeneratedCardToCombat(curse, PileType.Draw, card.Owner, CardPilePosition.Random);
		SpoonLog.Info($"forged curses: Contagious {card.Id.Entry} added {canonical.Id.Entry}");
	}

	/// <summary>Exhausts a random card from the owner's hand.</summary>
	public static async Task Ravenous(PlayerChoiceContext ctx, CardModel card)
	{
		var hand = card.Owner.PlayerCombatState?.Hand.Cards.Where(c => c != card).ToList();
		if (hand is not { Count: > 0 }) return;
		var eaten = hand[PlayRng(card).NextInt(hand.Count)];
		await CardCmd.Exhaust(ctx, eaten);
	}
}

/// <summary>Spiteful: the curse's end-of-turn in-hand effect runs twice (Regret doubles its count instead: it resets it).</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnTurnEndInHandWrapper))]
public static class SpitefulTurnEndPatch
{
	private static readonly Func<CardModel, PlayerChoiceContext, Task> TurnEnd =
		AccessTools.MethodDelegate<Func<CardModel, PlayerChoiceContext, Task>>(AccessTools.Method(typeof(CardModel), "OnTurnEndInHand"));

	public static void Postfix(CardModel __instance, PlayerChoiceContext choiceContext, ref Task __result)
	{
		if (__instance.Type != CardType.Curse || __instance is Regret || !ForgedCurses.Applies(__instance)) return;
		int times = ForgedCurseRule.SpitefulTimes(CurseDefects.Get(__instance));
		if (times > 1) __result = Again(__result, __instance, choiceContext, times - 1);
	}

	private static async Task Again(Task first, CardModel card, PlayerChoiceContext ctx, int more)
	{
		await first;
		for (int i = 0; i < more; i++) await TurnEnd(card, ctx);
	}
}
