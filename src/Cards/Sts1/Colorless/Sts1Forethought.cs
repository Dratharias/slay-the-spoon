using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Forethought: hand cards go under the draw pile and cost 0 until played (X costs stay X).</summary>
public sealed class Sts1Forethought : Sts1ColorlessCard
{
	public Sts1Forethought()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int inHand = PileType.Hand.GetPile(Owner).Cards.Count;
		if (inHand == 0)
		{
			return;
		}
		CardSelectorPrefs prefs = IsUpgraded
			? new CardSelectorPrefs(new LocString("cards", Id.Entry + ".selectionScreenPromptAny"), 0, inHand)
			: new CardSelectorPrefs(SelectionScreenPrompt, 1);
		foreach (CardModel card in (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).ToList())
		{
			if (!card.EnergyCost.CostsX)
			{
				card.EnergyCost.SetUntilPlayed(0);
			}
			await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
