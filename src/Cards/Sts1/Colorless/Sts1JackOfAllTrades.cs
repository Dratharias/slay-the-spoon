using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Jack of All Trades: each colorless card is rolled on its own, so repeats can happen.</summary>
public sealed class Sts1JackOfAllTrades : Sts1ColorlessCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.JackOfAllTrades);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(1),
	};

	public Sts1JackOfAllTrades()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> pool = ModelDb.CardPool<ColorlessCardPool>()
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
		foreach (CardModel card in CardFactory.GetForCombat(Owner, pool, DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList())
		{
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
