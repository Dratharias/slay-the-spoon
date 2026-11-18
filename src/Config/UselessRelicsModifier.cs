using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.UselessRelics;

namespace SlaySpoon.Config;

/// <summary>
/// Useless Relics (Custom Run, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): in combat, a once-per-fight
/// relic that has done its job can be thrown at an enemy for 10 damage (<see cref="UselessRelicThrow"/>); it comes back
/// when the combat ends.
/// </summary>
public sealed class SpoonUselessRelics : ModifierModel
{
	public static bool IsActive(IRunState? run) => run?.Modifiers.OfType<SpoonUselessRelics>().Any() == true;

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		UselessRelicThrow.Reset();
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		UselessRelicThrow.Reset();
		return Task.CompletedTask;
	}
}
