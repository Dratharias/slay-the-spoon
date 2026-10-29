using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Metamorphosis, kept for event grants; same effect as StS2's.</summary>
public sealed class Sts1Metamorphosis : Sts1ColorlessCard
{
	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Metamorphosis);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(3),
	};

	public Sts1Metamorphosis()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> pool = Owner.Character.CardPool
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
			.Where(c => c.Type == CardType.Attack);
		foreach (CardModel card in CardFactory.GetForCombat(Owner, pool, DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardGeneration))
		{
			card.SetToFreeThisCombat();
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random));
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(2m);
	}
}
