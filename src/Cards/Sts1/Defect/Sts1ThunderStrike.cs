using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Defect;

/// <summary>One hit on a random enemy per Lightning its owner Channeled this combat.</summary>
public sealed class Sts1ThunderStrike : Sts1DefectCard
{
	private const string CalculatedHitsKey = "CalculatedHits";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>(),
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar(CalculatedHitsKey).WithMultiplier((CardModel card, Creature? _) =>
			CombatManager.Instance.History.Entries.OfType<OrbChanneledEntry>()
				.Count(e => e.Actor.Player == card.Owner && e.Orb is LightningOrb)),
	};

	public Sts1ThunderStrike()
		: base(3, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int hits = (int)((CalculatedVar)DynamicVars[CalculatedHitsKey]).Calculate(cardPlay.Target);
		if (hits <= 0)
		{
			return;
		}
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this)
			.TargetingRandomOpponents(CombatState!)
			.WithHitFx("vfx/vfx_attack_lightning")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
