using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Relics.Events;

/// <summary>
/// StS1 Enchiridion (Cursed Tome): at the start of each combat, a random Power of the holder's character goes into the
/// hand, free that turn (X costs stay X). The pool follows the card set mode.
/// </summary>
public sealed class Sts1Enchiridion : Sts1EventRelic
{
	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player != Owner || Owner.PlayerCombatState?.TurnNumber != 1) return;
		CardModel[] powers = Owner.Character.CardPool
			.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
			.Where(c => c.Type == CardType.Power).ToArray();
		if (powers.Length == 0) return;
		Flash();
		CardModel? card = CardFactory.GetDistinctForCombat(Owner, powers, 1, Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
		if (card == null) return;
		if (!card.EnergyCost.CostsX) card.SetToFreeThisTurn();
		await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
	}
}
