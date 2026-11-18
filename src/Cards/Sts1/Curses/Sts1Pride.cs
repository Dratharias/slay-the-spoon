using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Curses;

/// <summary>
/// StS1 Pride: costs 1, Innate, Exhaust; if it is still in the hand at the end of the turn, a fresh copy goes on top of
/// the draw pile. Playing it does nothing (it just exhausts).
/// </summary>
public sealed class Sts1Pride : Sts1CurseCard
{
	public Sts1Pride() : base(1, TargetType.Self) { }

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate, CardKeyword.Exhaust };

	public override bool HasTurnEndInHandEffect => true;

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		if (CombatState == null) return;
		await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Sts1Pride>(Owner), PileType.Draw, Owner, CardPilePosition.Top);
	}
}
