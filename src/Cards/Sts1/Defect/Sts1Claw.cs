using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Defect;

/// <summary>
/// StS1 Claw (3 (+2) damage, every StS1 Claw +2 this combat) replaces the StS2 card, whose
/// numbers mix stronger and weaker. It buffs StS1 Claws only, as StS2's buffs StS2's.
/// </summary>
public sealed class Sts1Claw : Sts1DefectCard
{
	private const string IncreaseKey = "Increase";

	private decimal _extraDamageFromClawPlays;

	private decimal ExtraDamageFromClawPlays
	{
		get => _extraDamageFromClawPlays;
		set
		{
			AssertMutable();
			_extraDamageFromClawPlays = value;
		}
	}

	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Claw);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(3m, ValueProp.Move),
		new DynamicVar(IncreaseKey, 2m),
	};

	public Sts1Claw()
		: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
			.Execute(choiceContext);
		decimal increase = DynamicVars[IncreaseKey].BaseValue;
		foreach (Sts1Claw claw in Owner.PlayerCombatState!.AllCards.OfType<Sts1Claw>())
		{
			claw.BuffFromClawPlay(increase);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}

	// A downgrade rebuilds the vars from the canonical values; keep this combat's growth.
	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		DynamicVars.Damage.BaseValue += ExtraDamageFromClawPlays;
	}

	private void BuffFromClawPlay(decimal extraDamage)
	{
		DynamicVars.Damage.BaseValue += extraDamage;
		ExtraDamageFromClawPlays += extraDamage;
	}
}
