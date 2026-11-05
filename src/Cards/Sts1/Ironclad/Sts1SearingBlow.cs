using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Ironclad;

/// <summary>Can be upgraded any number of times; the title shows +N past the first upgrade.</summary>
public sealed class Sts1SearingBlow : Sts1IroncladCard
{
	/// <summary>Unlimited in StS1; a large finite cap keeps any loop over levels bounded.</summary>
	public override int MaxUpgradeLevel => 99;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(Sts1Math.SearingBlowDamage(0), ValueProp.Move),
	};

	public Sts1SearingBlow()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// CurrentUpgradeLevel is already the new level here.
		DynamicVars.Damage.UpgradeValueBy(Sts1Math.SearingBlowDamage(CurrentUpgradeLevel) - Sts1Math.SearingBlowDamage(CurrentUpgradeLevel - 1));
	}
}
