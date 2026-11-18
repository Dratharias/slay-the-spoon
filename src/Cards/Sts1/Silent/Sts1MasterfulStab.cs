using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

public sealed class Sts1MasterfulStab : Sts1SilentCard
{
	private int _hpLosses;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(12m, ValueProp.Move),
	};

	/// <summary>Times the owner lost HP while this copy sat in hand, draw or discard pile this combat.</summary>
	private int HpLosses
	{
		get => _hpLosses;
		set
		{
			AssertMutable();
			_hpLosses = value;
		}
	}

	public Sts1MasterfulStab()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		if (Sts1Math.MasterfulStabCounts(delta, creature == Owner.Creature,
			Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard))
		{
			HpLosses++;
		}
		return Task.CompletedTask;
	}

	/// <summary>1 more per HP loss this copy saw.</summary>
	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card != this || HpLosses == 0)
		{
			return false;
		}
		modifiedCost = originalCost + HpLosses;
		return true;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
