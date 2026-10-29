using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Silent;

public sealed class Sts1Setup : Sts1SilentCard
{
	public Sts1Setup()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel? card = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
			context: choiceContext, player: Owner, filter: null, source: this)).FirstOrDefault();
		if (card != null)
		{
			card.EnergyCost.SetUntilPlayed(0);
			await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
		}
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
