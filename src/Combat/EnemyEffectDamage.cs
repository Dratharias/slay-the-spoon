using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Combat;

/// <summary>
/// Enemy damage that reaches <see cref="Config.SpoonMonsterDamage"/> with no enemy dealer: Status cards an enemy
/// added (Burn, Toxic…) and enemy powers that hurt their victim (Constrict, Galvanic).
/// </summary>
public static class EnemyEffectDamage
{
	private static readonly ConditionalWeakTable<CardModel, object> PlayerMade = new();

	/// <summary>The creature an enemy power is hurting right now; flows through the power's awaits.</summary>
	private static readonly AsyncLocal<Creature?> Victim = new();

	/// <summary>A Status card no player created: enemies add theirs without a creator, Overclock and Immolate with one.</summary>
	public static bool IsEnemyStatus(CardModel? card)
	{
		return card != null && card.Type == CardType.Status && !PlayerMade.TryGetValue(card, out _);
	}

	public static bool IsEnemyEffectOn(Creature? target)
	{
		return target != null && ReferenceEquals(Victim.Value, target);
	}

	internal static void MarkPlayerMade(IEnumerable<CardModel> cards)
	{
		foreach (CardModel card in cards)
		{
			PlayerMade.AddOrUpdate(card, PlayerMade);
		}
	}

	/// <summary>Set before the power's async method starts, restored after it returns: the state machine keeps the value.</summary>
	internal static void Enter(Creature? victim, out Creature? previous)
	{
		previous = Victim.Value;
		Victim.Value = victim;
	}

	internal static void Exit(Creature? previous)
	{
		Victim.Value = previous;
	}
}

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat))]
public static class PlayerMadeStatusPatch
{
	/// <summary>The list is materialized once, so a lazy caller doesn't create its cards twice.</summary>
	public static void Prefix(ref IEnumerable<CardModel> cards, Player? creator)
	{
		if (creator != null)
		{
			List<CardModel> list = cards.ToList();
			EnemyEffectDamage.MarkPlayerMade(list);
			cards = list;
		}
	}
}

[HarmonyPatch(typeof(ConstrictPower), nameof(ConstrictPower.AfterSideTurnEnd))]
public static class ConstrictDamagePatch
{
	public static void Prefix(ConstrictPower __instance, out Creature? __state)
	{
		EnemyEffectDamage.Enter(__instance.Owner, out __state);
	}

	public static void Postfix(Creature? __state)
	{
		EnemyEffectDamage.Exit(__state);
	}
}

[HarmonyPatch(typeof(GalvanicPower), nameof(GalvanicPower.AfterCardPlayed))]
public static class GalvanicDamagePatch
{
	public static void Prefix(CardPlay cardPlay, out Creature? __state)
	{
		EnemyEffectDamage.Enter(cardPlay.Card.Owner.Creature, out __state);
	}

	public static void Postfix(Creature? __state)
	{
		EnemyEffectDamage.Exit(__state);
	}
}
