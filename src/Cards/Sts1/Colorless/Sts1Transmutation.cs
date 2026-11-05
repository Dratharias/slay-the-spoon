using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Transmutation: X random colorless cards (upgraded when this is), free this turn.</summary>
public sealed class Sts1Transmutation : Sts1ColorlessCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override bool HasEnergyCostX => true;

	public Sts1Transmutation()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int count = ResolveEnergyXValue();
		if (count <= 0)
		{
			return;
		}
		IEnumerable<CardModel> pool = ModelDb.CardPool<ColorlessCardPool>()
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
		foreach (CardModel card in CardFactory.GetForCombat(Owner, pool, count, Owner.RunState.Rng.CombatCardGeneration).ToList())
		{
			if (IsUpgraded)
			{
				CardCmd.Upgrade(card);
			}
			card.SetToFreeThisTurn();
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
		}
	}
}
