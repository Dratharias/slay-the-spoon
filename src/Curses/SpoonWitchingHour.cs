using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Curses.Forged;

namespace SlaySpoon.Curses;

/// <summary>
/// The Witching Hour (<see cref="WitchingHourRule"/>): the mod's own curse, Eternal, Innate, Unplayable. On odd turns it is
/// unplayable even with the Unholy Grimoire, the owner's other cards cost 1 more while it is in hand, and from the draw pile
/// it is drawn 80 % of the time. Its Grimoire play (even turns) stuns the chosen enemy (<c>"Stun"</c> in the curse table).
/// It lives in the hidden StS1 curse pool for its pool and portrait; Overcursed hands it out in place of Ascender's Bane,
/// and Cursed Deck's Witching Pact (<see cref="WitchingPact"/>) for a boss relic. Forged (Forge + Cursed Deck), its
/// numbers follow its level: <c>Penalty</c>, <c>HpLoss</c> (odd-turn end in hand) and <c>Turns</c> (stun).
/// </summary>
public sealed class SpoonWitchingHour : Sts1CurseCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords =>
		new[] { CardKeyword.Eternal, CardKeyword.Innate, CardKeyword.Unplayable };

	public override bool HasCantPlayMessage => true;

	public override bool HasTurnEndInHandEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Penalty", WitchingHourRule.CostPenalty), new DynamicVar("HpLoss", 0), new DynamicVar("Turns", 1),
	};

	private bool InForgedRun => ForgedCurses.Applies(this);

	public int HpLoss => WitchingHourRule.HpLossAt(CurrentUpgradeLevel, InForgedRun);

	public int StunTurns => WitchingHourRule.StunTurnsAt(CurrentUpgradeLevel);

	/// <summary>Brings the text vars to the current level (after an upgrade, a fusion or a load).</summary>
	internal void RefreshVars()
	{
		SetTo("Penalty", WitchingHourRule.CostPenaltyAt(CurrentUpgradeLevel));
		SetTo("HpLoss", HpLoss);
		SetTo("Turns", StunTurns);
	}

	private void SetTo(string name, decimal value)
	{
		if (DynamicVars.TryGetValue(name, out var v) && v.BaseValue != value) v.BaseValue = value;
	}

	protected override void OnUpgrade()
	{
		base.OnUpgrade();
		RefreshVars();
	}

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		if (CombatState is not { } combat || !WitchingHourRule.IsOddTurn(combat.RoundNumber)) return;
		RefreshVars();
		if (HpLoss <= 0) return;
		await Cmd.Wait(0.25f);
		await CreatureCmd.Damage(choiceContext, Owner.Creature, HpLoss, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, null);
	}

	/// <summary>The Grimoire play: the enemy skips its next <see cref="StunTurns"/> turns, then resumes its own pattern.</summary>
	internal static Task Stun(Creature target, int turns)
	{
		if (target.Monster is not { } monster) return Task.CompletedTask;
		string? next = monster.MoveStateMachine?.StateLog.LastOrDefault(s => s.Id != "STUNNED")?.Id;
		return StunFor(target, Math.Max(1, turns), next);
	}

	private static Task StunFor(Creature target, int turns, string? next) =>
		turns <= 1 ? CreatureCmd.Stun(target, next) : CreatureCmd.Stun(target, _ => StunFor(target, turns - 1, next), next);

	protected override bool IsPlayable => CombatState is not { } combat || !WitchingHourRule.BlocksPlay(combat.RoundNumber);

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card == this || card.Owner != Owner || CombatState is not { } combat) return false;
		modifiedCost = WitchingHourRule.CostWith(originalCost, combat.RoundNumber, Pile?.Type == PileType.Hand, CurrentUpgradeLevel);
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

	/// <summary>Levels change outside <c>OnUpgrade</c> too (fusion, load): the text vars follow before each draw.</summary>
	public static void Prefix(CardModel __instance)
	{
		if (__instance is SpoonWitchingHour { IsCanonical: false } curse) curse.RefreshVars();
	}

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
