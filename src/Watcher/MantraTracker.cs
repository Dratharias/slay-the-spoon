using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>
/// Per-player, per-combat Mantra counter for Brilliance. It hangs off the player's <see cref="PlayerCombatState"/>,
/// which is new each combat, so it resets by itself and never leaks between players in co-op.
/// </summary>
public static class MantraTracker
{
	private static readonly ConditionalWeakTable<PlayerCombatState, MantraTally> Tallies = new();

	public static int Gained(Player player) =>
		player.PlayerCombatState is { } combat && Tallies.TryGetValue(combat, out MantraTally? tally) ? tally.Total : 0;

	public static void Record(Player player, int amount)
	{
		if (player.PlayerCombatState is { } combat)
		{
			Tallies.GetOrCreateValue(combat).Add(amount);
		}
	}
}
