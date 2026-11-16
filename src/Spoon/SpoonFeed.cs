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
/// Spoon-Feed ("Becquée", spec §6): handed to each supporter at the start of the Spoon fight. The supporter gives up to
/// <see cref="SpoonRule.GiftMaxHp"/> max HP to a fighter, keeping at least 1 (<see cref="SpoonRule.GiftAmount"/>).
/// Retained, never exhausted, combat-only.
/// </summary>
public sealed class SpoonSpoonFeed : SpoonCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(SpoonRule.GiftMaxHp)];

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

	/// <summary>Only while a fighter is alive to receive it and the giver has more than 1 max HP.</summary>
	protected override bool IsPlayable =>
		SpoonRule.CanGift(Owner.Creature.MaxHp) && SpoonFight.Fighters(CombatState).Any(c => c.IsAlive);

	public SpoonSpoonFeed()
		: base(1, CardType.Skill, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		if (!SpoonRule.CanGift(Owner.Creature.MaxHp)) return;
		int before = Owner.Creature.MaxHp;
		// Never below 1 max HP: LoseMaxHp's damage step then leaves the giver at the new max HP (at least 1), unblockable
		// and unpowered, so powered damage modifiers such as Vulnerable do not apply.
		await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, SpoonRule.GiftAmount(before), true);
		// The fighter gains what the giver actually lost, so the gift never creates max HP.
		int transferred = SpoonRule.GiftTransferred(before, Owner.Creature.MaxHp);
		if (transferred > 0 && cardPlay.Target.IsAlive) await CreatureCmd.GainMaxHp(cardPlay.Target, transferred);
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
