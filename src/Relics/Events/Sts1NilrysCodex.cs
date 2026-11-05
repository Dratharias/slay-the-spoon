using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Relics.Events;

/// <summary>
/// StS1 Nilry's Codex (Cursed Tome): at the end of each turn, the holder may shuffle 1 of 3 random cards of their
/// character into the draw pile. Asked at the end-of-turn flush, as vanilla Well-Laid Plans does.
/// </summary>
public sealed class Sts1NilrysCodex : Sts1EventRelic
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(3) };

	public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner || Owner.Creature.IsDead || player.Creature.CombatState is not { } combat || !Hook.ShouldFlush(combat, player)) return;
		List<CardModel> cards = CardFactory.GetDistinctForCombat(Owner,
			Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
			DynamicVars.Cards.IntValue, Owner.RunState.Rng.CombatCardGeneration).ToList();
		if (cards.Count == 0) return;
		Flash();
		CardModel? card = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner, canSkip: true);
		if (card != null)
		{
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random);
		}
	}
}
