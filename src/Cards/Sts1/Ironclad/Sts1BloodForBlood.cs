using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Ironclad;

/// <summary>Costs 1 less for each time the owner lost HP this combat, wherever the card sits.</summary>
public sealed class Sts1BloodForBlood : Sts1IroncladCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(18m, ValueProp.Move),
	};

	public Sts1BloodForBlood()
		: base(4, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != this || IsClone)
		{
			return Task.CompletedTask;
		}
		// A copy created mid-combat starts with the HP losses that already happened.
		int losses = CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
			.Count(e => e.Receiver == Owner.Creature && e.Result.UnblockedDamage > 0);
		EnergyCost.AddThisCombat(-losses);
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == Owner.Creature && result.UnblockedDamage > 0)
		{
			EnergyCost.AddThisCombat(-1);
		}
		return Task.CompletedTask;
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
		DynamicVars.Damage.UpgradeValueBy(4m);
		EnergyCost.UpgradeBy(-1);
	}
}
