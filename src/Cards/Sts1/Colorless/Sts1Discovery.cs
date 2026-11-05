using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Discovery: the choice can't be skipped, Exhaust (removed on upgrade).</summary>
public sealed class Sts1Discovery : Sts1ColorlessCard
{
	/// <summary>Retired (owner, 2026-11-05): the StS2 card serves every mode; kept for old saves.</summary>
	public override Sts1Relation Relation => Sts1Relation.Retired;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Discovery);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Sts1Discovery()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> cards = CardFactory.GetDistinctForCombat(Owner,
			Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
			3, Owner.RunState.Rng.CombatCardGeneration).ToList();
		CardModel? card = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner, canSkip: false);
		if (card != null)
		{
			card.SetToFreeThisTurn();
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
