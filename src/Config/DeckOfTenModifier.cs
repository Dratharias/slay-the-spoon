using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.DeckOfTen;

namespace SlaySpoon.Config;

/// <summary>
/// Deck of 10 (Custom Run, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): every player starts with the
/// Deck of 10 relic (<see cref="SpoonDeckOfTenRelic"/>), which does the trimming.
/// </summary>
public sealed class SpoonDeckOfTen : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonDeckOfTen);

	protected override void AfterRunCreated(RunState runState)
	{
		var relic = ModelDb.Relic<SpoonDeckOfTenRelic>();
		foreach (Player player in runState.Players)
		{
			player.RelicGrabBag.Remove(relic);
			if (player.Relics.All(r => r.Id != relic.Id)) player.AddRelicInternal(relic.ToMutable());
		}
		runState.SharedRelicGrabBag.Remove(relic);
	}
}
