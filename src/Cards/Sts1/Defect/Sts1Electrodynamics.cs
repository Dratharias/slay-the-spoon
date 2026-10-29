using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1Electrodynamics : Sts1DefectCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Orbs", 2m),
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.FromPower<Sts1ElectrodynamicsPower>(),
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>(),
	};

	public Sts1Electrodynamics()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
		if (!Owner.Creature.HasPower<Sts1ElectrodynamicsPower>())
		{
			await PowerCmd.Apply<Sts1ElectrodynamicsPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}
		for (int i = 0; i < DynamicVars["Orbs"].IntValue; i++)
		{
			await OrbCmd.Channel<LightningOrb>(choiceContext, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Orbs"].UpgradeValueBy(1m);
	}
}
