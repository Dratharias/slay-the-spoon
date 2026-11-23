using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Overcursed (Custom Run, docs/superpowers/specs/2026-11-23-overcursed-design.md): Matriochka for curses only. Every curse
/// that enters a deck brings two copies; "?" events offer "Embrace the curse" (as in Cursed Deck).
/// </summary>
public sealed class SpoonOvercursed : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonOvercursed);

	private HashSet<CardModel> _cardsToSkip = new();

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_cardsToSkip = new HashSet<CardModel>();
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (oldPileType != PileType.None) return;
		CardPile? pile = card.Pile;
		if (pile == null || pile.Type != PileType.Deck || clonedBy != null || _cardsToSkip.Remove(card)) return;
		bool copied = RunState.Modifiers.Any(m => m is Hoarder or SpoonMatriochka);
		int copies = OvercursedRule.CopiesFor(card.Type == CardType.Curse, copied, card.Owner.Deck.Cards.Count);
		for (int i = 0; i < copies; i++)
		{
			CardModel copy = card.Owner.RunState.CloneCard(card);
			_cardsToSkip.Add(copy);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(copy, PileType.Deck, CardPilePosition.Bottom, this));
		}
	}
}
