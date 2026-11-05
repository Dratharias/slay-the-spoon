using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Relics.Events;

/// <summary>StS1 Warped Tongs (Ominous Forge): at the start of each turn, after the draw, a random card in hand is upgraded for the combat.</summary>
public sealed class Sts1WarpedTongs : Sts1EventRelic
{
	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner) return Task.CompletedTask;
		CardModel[] upgradable = PileType.Hand.GetPile(Owner).Cards.Where(c => c.IsUpgradable).ToArray();
		if (upgradable.Length == 0) return Task.CompletedTask;
		CardModel? card = Owner.RunState.Rng.CombatCardSelection.NextItem(upgradable);
		if (card == null) return Task.CompletedTask;
		Flash();
		CardCmd.Upgrade(card);
		return Task.CompletedTask;
	}
}
