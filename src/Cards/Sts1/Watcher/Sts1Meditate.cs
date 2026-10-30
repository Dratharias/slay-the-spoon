using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Meditate: put Magic cards from the owner's discard pile into their hand and Retain them this turn, enter Calm, end the turn.</summary>
public sealed class Sts1Meditate : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<Sts1CalmStancePower>(), HoverTipFactory.FromKeyword(CardKeyword.Retain) };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 1m),
	};

	public Sts1Meditate()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPile discard = PileType.Discard.GetPile(Owner);
		int count = DynamicVars["Magic"].IntValue;
		List<CardModel> picked;
		if (discard.Cards.Count <= count)
		{
			picked = discard.Cards.ToList();
		}
		else
		{
			picked = (await CardSelectCmd.FromCombatPile(choiceContext, discard, Owner, new CardSelectorPrefs(SelectionScreenPrompt, count))).ToList();
		}
		foreach (CardModel card in picked)
		{
			if (PileType.Hand.GetPile(Owner).Cards.Count >= CardPile.MaxCardsInHand)
			{
				break;
			}
			await CardPileCmd.Add(card, PileType.Hand);
			card.GiveSingleTurnRetain();
		}
		await StanceCmd.Enter(Owner, Stance.Calm, this, choiceContext);
		PlayerCmd.EndTurn(Owner, canBackOut: false);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}
}
