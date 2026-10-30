using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1 Vault: the owner takes an extra turn. Like Pael's Eye, the game gives the extra turn to the players whose models say
/// so (here: only the owner), and the other players' turns are skipped.
/// </summary>
public sealed class Sts1ExtraTurnPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(HammerTimePower);

	public override bool ShouldTakeExtraTurn(Player player) => player == Owner.Player;

	public override async Task AfterTakingExtraTurn(Player player)
	{
		if (player == Owner.Player)
		{
			Flash();
			await PowerCmd.Remove(this);
		}
	}
}
