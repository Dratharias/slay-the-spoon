using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Cursed Deck (spec "Curses against the enemy"): in combat, the curses of a player holding the Unholy Grimoire are
/// playable. The keywords (Unplayable dropped, Exhaust added) come from the Grimoire's
/// <see cref="SpoonUnholyGrimoire.TryModifyKeywordsInCombat"/>; this file gives the target and the cost. Outside the
/// mode, out of combat or without the Grimoire, every check below returns early and the curse stays vanilla.
/// </summary>
public static class CursePlay
{
	/// <summary>Curse type, mutable card in a combat pile, Cursed Deck run, owner holds a working Grimoire.</summary>
	public static bool IsPlayableCurse(CardModel? card)
	{
		if (card == null || card.IsCanonical || card.Type != CardType.Curse || card.CombatState == null) return false;
		var owner = card.Owner;
		if (owner == null) return false;
		return CurseRule.IsPlayable(SpoonCursedDeck.IsActive(owner.RunState),
			owner.GetRelic<SpoonUnholyGrimoire>() is { IsMelted: false });
	}

	public static CurseRow? RowFor(CardModel card) => CurseTable.For(card.Id.Entry);
}

/// <summary>A playable curse targets one enemy (its played effect hits that enemy only); a curse with no played effect
/// needs no target (the Grimoire's exhaust damage hits every enemy), so it shows no arrow.</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.TargetType), MethodType.Getter)]
public static class PlayableCurseTargetPatch
{
	public static void Postfix(CardModel __instance, ref TargetType __result)
	{
		if (__instance.Type != CardType.Curse) return;
		if (CursePlay.IsPlayableCurse(__instance))
			__result = CurseRule.NeedsTarget(CursePlay.RowFor(__instance)) ? TargetType.AnyEnemy : TargetType.AllEnemies;
	}
}

/// <summary>
/// A vanilla-unplayable curse has a −1 base cost, which <see cref="CardEnergyCost.GetWithModifiers"/> returns before any
/// modifier; for a playable curse the cost comes from <see cref="CurseRule.PlayedCost"/> (then the global cost hooks, like
/// any card). The same value drives the card's cost orb, the energy check and the energy spent.
/// </summary>
[HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.GetWithModifiers))]
public static class PlayableCurseCostPatch
{
	private static readonly AccessTools.FieldRef<CardEnergyCost, CardModel> CardOf =
		AccessTools.FieldRefAccess<CardEnergyCost, CardModel>("_card");

	public static void Postfix(CardEnergyCost __instance, CostModifiers modifiers, ref int __result)
	{
		if (__instance.CostsX) return;
		CardModel card = CardOf(__instance);
		if (card == null || card.Type != CardType.Curse || !CursePlay.IsPlayableCurse(card)) return;
		int canonical = __instance.Canonical;
		int? cost = CurseRule.PlayedCost(CursePlay.RowFor(card), canonical, card.IsUpgraded, __result);
		if (cost == null) return;
		int value = ForgedCurseRule.CostWithDefects(Forged.CurseDefects.Get(card), cost.Value, playable: true);
		if (canonical < 0 && modifiers.HasFlag(CostModifiers.Global) && card.CombatState is { } combat)
			value = (int)Hook.ModifyEnergyCostInCombat(combat, card, value);
		__result = Math.Max(0, value);
	}
}
