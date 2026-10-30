using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Simmering Rage: enter Wrath at the start of the owner's next turn, before their draw.</summary>
public sealed class Sts1WrathNextTurnPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(EnragePower);

	public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		await StanceCmd.Enter(player, Stance.Wrath, null, choiceContext);
		await PowerCmd.Remove(this);
	}
}
