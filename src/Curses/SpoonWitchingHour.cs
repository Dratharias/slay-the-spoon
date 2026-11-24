using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using SlaySpoon.Cards;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// The Witching Hour (<see cref="WitchingHourRule"/>): the mod's own curse, Eternal, Innate, Unplayable. On odd turns it is
/// unplayable even with the Unholy Grimoire, the owner's other cards cost 1 more while it is in hand, and from the draw pile
/// it is drawn 80 % of the time. Its Grimoire play (even turns) stuns the chosen enemy (<c>"Stun"</c> in the curse table).
/// It lives in the hidden StS1 curse pool for its pool and portrait; Overcursed hands it out in place of Ascender's Bane,
/// and Cursed Deck's Witching Pact (<see cref="WitchingPact"/>) for a boss relic.
/// </summary>
public sealed class SpoonWitchingHour : Sts1CurseCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords =>
		new[] { CardKeyword.Eternal, CardKeyword.Innate, CardKeyword.Unplayable };

	public override bool HasCantPlayMessage => true;

	protected override bool IsPlayable => CombatState is not { } combat || !WitchingHourRule.BlocksPlay(combat.RoundNumber);

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card == this || card.Owner != Owner || CombatState is not { } combat) return false;
		modifiedCost = WitchingHourRule.CostWith(originalCost, combat.RoundNumber, Pile?.Type == PileType.Hand);
		return modifiedCost != originalCost;
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player != Owner || Pile is not { Type: PileType.Draw } pile) return;
		int round = combatState.RoundNumber;
		if (!WitchingHourRule.IsOddTurn(round) || round <= 1) return;
		var run = Owner.RunState;
		var rng = new Rng(run.Rng.Seed, WitchingHourRule.PullStream(run.TotalFloor, round, pile.Cards.ToList().IndexOf(this)));
		if (!WitchingHourRule.Pulls(round, rng.NextInt(100))) return;
		await CardPileCmd.Add(this, PileType.Hand);
	}
}

/// <summary>Overcursed: the Witching Hour takes the place of the Ascender's Bane the ascension just added to the deck.</summary>
[HarmonyPatch(typeof(AscensionManager), nameof(AscensionManager.ApplyEffectsTo))]
public static class WitchingHourAscensionPatch
{
	public static void Postfix(Player player)
	{
		if (!SpoonOvercursed.IsActive(player.RunState)) return;
		var bane = player.Deck.Cards.LastOrDefault(c => c is AscendersBane);
		if (bane == null) return;
		int index = player.Deck.Cards.ToList().IndexOf(bane);
		player.Deck.RemoveInternal(bane, silent: true);
		var curse = player.RunState.CreateCard<SpoonWitchingHour>(player);
		curse.FloorAddedToDeck = 1;
		player.Deck.AddInternal(curse, index, silent: true);
		SpoonLog.Info("overcursed: Ascender's Bane replaced by the Witching Hour");
	}
}

/// <summary>
/// The card's own text already says "On odd turns: Unplayable", so the engine's leading "Unplayable." keyword line is
/// dropped from its description (owner, 2026-11-24). The keyword itself stays: it blocks the play without the Grimoire
/// and keeps vanilla filters (enchantments, afflictions, "draw a playable card" effects) treating it as unplayable.
/// </summary>
[HarmonyPatch]
public static class WitchingHourKeywordLinePatch
{
	public static System.Reflection.MethodBase TargetMethod() =>
		AccessTools.GetDeclaredMethods(typeof(CardModel)).First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

	public static void Postfix(CardModel __instance, ref string __result)
	{
		if (__instance is not SpoonWitchingHour) return;
		var lines = __result.Split('\n').ToList();
		int index = lines.IndexOf(UnplayableLine());
		if (index < 0) return;
		lines.RemoveAt(index);
		__result = string.Join('\n', lines);
	}

	/// <summary>The engine's keyword line (<c>CardKeywordExtensions.GetCardText</c>, internal): gold title, then the period.</summary>
	private static string UnplayableLine() =>
		"[gold]" + new LocString("card_keywords", "UNPLAYABLE.title").GetFormattedText() + "[/gold]"
		+ new LocString("card_keywords", "PERIOD").GetRawText();
}
