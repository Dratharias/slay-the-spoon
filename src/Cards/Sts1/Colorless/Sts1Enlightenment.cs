using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Enlightenment: no Exhaust, unlike StS2's.</summary>
public sealed class Sts1Enlightenment : Sts1ColorlessCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Enlightenment);

	public Sts1Enlightenment()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards)
		{
			if (IsUpgraded)
			{
				card.EnergyCost.SetThisCombat(1, reduceOnly: true);
			}
			else
			{
				card.EnergyCost.SetThisTurnOrUntilPlayed(1, reduceOnly: true);
			}
		}
		return Task.CompletedTask;
	}
}
