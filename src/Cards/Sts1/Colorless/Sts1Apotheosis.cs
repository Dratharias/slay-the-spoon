using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Apotheosis: costs 2 (1 upgraded) and Exhausts; StS2's is Innate.</summary>
public sealed class Sts1Apotheosis : Sts1ColorlessCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Apotheosis);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Sts1Apotheosis()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (CardModel card in Owner.PlayerCombatState!.AllCards)
		{
			if (card != this && card.IsUpgradable)
			{
				CardCmd.Upgrade(card);
			}
		}
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
