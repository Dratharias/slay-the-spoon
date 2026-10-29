using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Ironclad;

/// <summary>StS1's weaker Rampage (8 damage, +5 / +8 per play) replaces the StS2 card.</summary>
public sealed class Sts1Rampage : Sts1IroncladCard
{
	private const string IncreaseKey = "Increase";

	private decimal _extraDamageFromPlays;

	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(Rampage);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
		new DynamicVar(IncreaseKey, 5m),
	};

	/// <summary>Kept so a downgrade (which resets the vars) can restore the damage gained this combat.</summary>
	private decimal ExtraDamageFromPlays
	{
		get => _extraDamageFromPlays;
		set
		{
			AssertMutable();
			_extraDamageFromPlays = value;
		}
	}

	public Sts1Rampage()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		DynamicVars.Damage.BaseValue += DynamicVars[IncreaseKey].BaseValue;
		ExtraDamageFromPlays += DynamicVars[IncreaseKey].BaseValue;
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		DynamicVars.Damage.BaseValue += ExtraDamageFromPlays;
	}

	protected override void OnUpgrade()
	{
		DynamicVars[IncreaseKey].UpgradeValueBy(3m);
	}
}
