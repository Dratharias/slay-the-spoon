using MegaCrit.Sts2.Core.Entities.Players;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>
/// A power, relic or card that reacts when a player changes stance (StS1's onChangeStance and switchedStance).
/// <see cref="StanceCmd.Enter"/> calls it on the player's own powers and relics and on the cards in their hand, draw
/// pile and discard pile, after the energy for the change is granted.
/// </summary>
public interface IStanceListener
{
	Task AfterStanceChanged(Player player, Stance from, Stance to);
}
