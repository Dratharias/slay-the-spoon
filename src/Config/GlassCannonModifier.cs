using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.GlassCannon;

namespace SlaySpoon.Config;

/// <summary>
/// Glass Cannon (Custom Run, docs/superpowers/specs/2026-11-19-glass-cannon-design.md): every player starts with the
/// Glass Heart relic (<see cref="SpoonGlassHeart"/>, which carries the rules), Vajra and Girya.
/// </summary>
public sealed class SpoonGlassCannon : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonGlassCannon);

	protected override void AfterRunCreated(RunState runState)
	{
		var relics = new List<RelicModel> { ModelDb.Relic<SpoonGlassHeart>() };
		relics.AddRange(GlassCannonRule.StartRelics
			.Select(id => ModelDb.AllRelics.FirstOrDefault(m => m.Id.Entry == id))
			.Where(m => m != null)!);
		// None of the three has a pickup effect (AfterObtained does not run for relics added at creation).
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
