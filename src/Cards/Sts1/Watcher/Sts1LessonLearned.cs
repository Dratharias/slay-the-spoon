using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Lesson Learned: a fatal hit upgrades a random upgradable card in the owner's deck.</summary>
public sealed class Sts1LessonLearned : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Fatal) };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(10m, ValueProp.Move),
	};

	public Sts1LessonLearned()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		// Snapshot Fatal eligibility before damage removes powers from a dying target (same as Feed).
		bool eligible = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
		AttackCommand attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
		if (eligible && attack.Results.SelectMany(hits => hits).Any(hit => hit.WasTargetKilled))
		{
			// Permanent, as in StS1: the deck copy is upgraded. Deck order and the synced run RNG keep co-op peers in agreement.
			List<CardModel> upgradable = Owner.Deck.Cards.Where(c => c.IsUpgradable).ToList();
			CardModel? chosen = Owner.RunState.Rng.CombatCardSelection.NextItem(upgradable);
			if (chosen != null)
			{
				UpgradeDeckCard(chosen);
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}

	// CardCmd.Upgrade does nothing once the last enemy is dead and combat is ending, which is the usual case for a
	// kill. In that case do what it does for a deck card (history entry, upgrade, finalize) without the guard or the preview.
	private void UpgradeDeckCard(CardModel card)
	{
		if (!CombatManager.Instance.IsEnding)
		{
			CardCmd.Upgrade(card);
			return;
		}
		if (!card.IsUpgradable)
		{
			return;
		}
		Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(Owner.NetId).UpgradedCards.Add(card.Id);
		card.UpgradeInternal();
		card.FinalizeUpgradeInternal();
	}
}
