using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Devotion: after the owner draws at turn start, gain Amount Mantra (straight into Divinity when none is held and Amount is 10 or more).</summary>
public sealed class Sts1DevotionPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(RitualPower);

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
		{
			return;
		}
		Flash();
		if (WatcherPowerMath.DevotionEntersDivinity(Owner.HasPower<Sts1MantraPower>(), Amount))
		{
			await StanceCmd.Enter(player, Stance.Divinity, null, choiceContext);
		}
		else
		{
			await StanceCmd.GainMantra(player, Amount, null, choiceContext);
		}
	}
}
