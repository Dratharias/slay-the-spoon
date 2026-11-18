using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Curses;

/// <summary>
/// StS1 Parasite: Unplayable; removing it from the deck costs its owner <c>MaxHp</c> max HP. StS2 has no Parasite, so it
/// lives in the hidden StS1 curse pool. The amount is a card var so the Cursed Deck table can raise it when upgraded.
/// </summary>
public sealed class Sts1Parasite : Sts1CurseCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new MaxHpVar(3m) };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

	public override async Task BeforeCardRemoved(CardModel card)
	{
		if (card != this) return;
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.MaxHp.IntValue, isFromCard: false);
	}
}
