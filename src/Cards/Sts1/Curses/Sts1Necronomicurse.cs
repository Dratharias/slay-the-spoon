using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Cards.Sts1.Curses;

/// <summary>
/// StS1 Necronomicurse, from the Necronomicon: Unplayable and Eternal (StS1 puts it back whenever it leaves the deck);
/// exhausting it adds a copy to the hand.
/// </summary>
public sealed class Sts1Necronomicurse : Sts1CurseCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Eternal, CardKeyword.Unplayable };

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card != this || CombatState == null) return;
		Owner.GetRelic<Sts1Necronomicon>()?.Flash();
		await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Sts1Necronomicurse>(Owner), PileType.Hand, Owner);
	}
}
