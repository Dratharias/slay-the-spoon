using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// Spoon-Feed ("Becquée", spec §6): handed to each supporter at the start of the Spoon fight. The supporter gives
/// <see cref="SpoonRule.GiftMaxHp"/> max HP to a fighter. Retained, never exhausted, combat-only.
/// </summary>
public sealed class SpoonSpoonFeed : SpoonCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(SpoonRule.GiftMaxHp)];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

	/// <summary>Only while a fighter is alive to receive it.</summary>
	protected override bool IsPlayable => SpoonFight.Fighters(CombatState).Any(c => c.IsAlive);

	public SpoonSpoonFeed()
		: base(1, CardType.Skill, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		decimal amount = DynamicVars.MaxHp.BaseValue;
		await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, amount, true);
		if (cardPlay.Target.IsAlive) await CreatureCmd.GainMaxHp(cardPlay.Target, amount);
	}
}

/// <summary>
/// AnyAlly cards accept any living creature on the player's side (CardModel.IsValidTarget). Spoon-Feed accepts a living
/// fighter only. The targeting arrow may still rest on a supporter or a summon, but the play is refused: both the
/// manual play check and the queued play action validate the target through this method.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
internal static class SpoonFeedTargetPatch
{
	[HarmonyPostfix]
	private static void FightersOnly(CardModel __instance, Creature? target, ref bool __result)
	{
		if (__result && __instance is SpoonSpoonFeed && !(target is { IsPlayer: true } && SpoonFight.IsFighter(target)))
			__result = false;
	}
}
