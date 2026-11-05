using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Rushdown: whenever the owner enters Wrath, draw Amount cards.</summary>
public sealed class Sts1RushdownPower : PowerModel, IStanceListener, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(DrawCardsNextTurnPower);

	public async Task AfterStanceChanged(PlayerChoiceContext choiceContext, Player player, Stance from, Stance to)
	{
		if (player != Owner.Player || from == to || to != Stance.Wrath)
		{
			return;
		}
		Flash();
		await CardPileCmd.Draw(choiceContext, Amount, player);
	}
}
