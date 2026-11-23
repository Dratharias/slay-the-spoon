using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Unholy Grimoire (owner, 2026-11-18), the first relic of the future curse deck mode: whenever one of the holder's
/// curses is exhausted, every enemy takes 5 damage (<see cref="CurseRule.GrimoireDamageFor"/>: 12 for an upgraded curse with no played effect).
/// In Cursed Deck runs it also makes the holder's curses playable against one enemy (<see cref="CursePlay"/>). Art: a Pixabay
/// illustration (sources in docs/backlog.md).
/// </summary>
public sealed class SpoonUnholyGrimoire : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_unholy_grimoire";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Damage", CurseRule.GrimoireDamage), new DynamicVar("DamagePlus", CurseRule.GrimoireNoEffectPlus) };

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner != Owner || card.Type != CardType.Curse || Owner.Creature.CombatState is not { } combat) return;
		var enemies = combat.HittableEnemies.ToList();
		if (enemies.Count == 0) return;
		Flash();
		int damage = CurseRule.GrimoireDamageFor(CurseTable.For(card.Id.Entry), card.CurrentUpgradeLevel);
		await CreatureCmd.Damage(choiceContext, enemies, damage, ValueProp.Unpowered, Owner.Creature);
	}

	/// <summary>Cursed Deck: the holder's curses lose Unplayable and gain Exhaust (see <see cref="CursePlay"/>).</summary>
	public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
	{
		if (card.Type != CardType.Curse || card.Owner != Owner || !CursePlay.IsPlayableCurse(card)) return false;
		bool removed = keywords.Remove(CardKeyword.Unplayable);
		bool added = keywords.Add(CardKeyword.Exhaust);
		return removed || added;
	}

	/// <summary>
	/// Cursed Deck: a played curse's row effect hits the chosen enemy. Runs before the curse reaches the exhaust pile
	/// (the engine exhausts after the play hooks), so the hand count already excludes it.
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.Owner != Owner || card.Type != CardType.Curse || !CursePlay.IsPlayableCurse(card)) return;
		if (CursePlay.RowFor(card) is not { } row || row.Played == "None") return;
		if (cardPlay.Target is not { IsAlive: true } target || target.Side == Owner.Creature.Side) return;
		if (card.CombatState is not { } combat) return;
		int hand = Owner.PlayerCombatState?.Hand.Cards.Count ?? 0;
		int played = CombatManager.Instance.History.CardPlaysStarted
			.Count(e => e.HappenedThisTurn(combat) && e.CardPlay.Player == Owner);
		int amount = CurseRule.PlayedAmount(row, card.CurrentUpgradeLevel, hand, played);
		if (amount <= 0) return;
		Flash();
		switch (row.Played)
		{
			case "HpLoss":
			case "HpLossPerHandCard":
			case "HpLossPerCardPlayed":
				await CreatureCmd.Damage(choiceContext, target, amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, card, cardPlay);
				break;
			case "Damage":
				await CreatureCmd.Damage(choiceContext, target, amount, ValueProp.Unpowered, Owner.Creature, card, cardPlay);
				break;
			case "Weak":
				await PowerCmd.Apply<WeakPower>(choiceContext, target, amount, Owner.Creature, card);
				break;
			case "Vulnerable":
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target, amount, Owner.Creature, card);
				break;
			case "GainGold":
				await PlayerCmd.GainGold(amount, Owner);
				break;
			case "MaxHpLoss":
				await CreatureCmd.LoseMaxHp(choiceContext, target, amount, isFromCard: true);
				break;
			default:
				SpoonLog.Error($"cursed deck: unknown played effect {row.Played} for {row.Id}");
				break;
		}
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class UnholyGrimoirePoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonUnholyGrimoire>()).ToList();
	}
}
