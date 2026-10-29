using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1ForceField : Sts1DefectCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(12m, ValueProp.Move),
	};

	public override bool GainsBlock => true;

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card == this && !IsClone)
		{
			int powers = CombatManager.Instance.History.CardPlaysFinished.Count(e =>
				e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Power);
			EnergyCost.AddThisCombat(-powers);
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Power)
		{
			EnergyCost.AddThisCombat(-1);
		}
		return Task.CompletedTask;
	}

	public Sts1ForceField()
		: base(4, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(4m);
	}
}
