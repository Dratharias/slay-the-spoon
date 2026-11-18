using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Achilles;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Achilles' Heel (Custom Run, docs/superpowers/specs/2026-11-18-achilles-heel-design.md): every player starts with the
/// Achilles' Heel relic (<see cref="SpoonAchillesHeel"/>, which carries the rules) and the three common Block relics.
/// </summary>
public sealed class SpoonAchillesHeelMode : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonAchillesHeelMode);

	protected override void AfterRunCreated(RunState runState)
	{
		var relics = new List<RelicModel> { ModelDb.Relic<SpoonAchillesHeel>() };
		relics.AddRange(AchillesRule.StartRelics
			.Select(id => ModelDb.AllRelics.FirstOrDefault(m => m.Id.Entry == id))
			.Where(m => m != null)!);
		// None of the four has a pickup effect (AfterObtained does not run for relics added at creation).
		foreach (Player player in runState.Players)
			foreach (RelicModel canonical in relics)
			{
				player.RelicGrabBag.Remove(canonical);
				if (player.Relics.Any(r => r.Id == canonical.Id)) continue;
				player.AddRelicInternal(canonical.ToMutable());
			}
		foreach (RelicModel canonical in relics)
			runState.SharedRelicGrabBag.Remove(canonical);
	}
}
