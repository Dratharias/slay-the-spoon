using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace SlaySpoon.Config;

/// <summary>
/// Run flag: a run started from the main-menu "Test: The Spoon" button (<see cref="Spoon.SpoonTestStart"/>). It is
/// saved with the run, so a reload keeps it, and <see cref="Spoon.SpoonTestProgressGuards"/> keep such a run out of the
/// modded profile's progress. No Custom Run row offers it.
/// </summary>
public sealed class SpoonTestRun : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonTestRun);

	public static bool IsActive(SerializableRun? run)
	{
		if (run == null) return false;
		var id = ModelDb.Modifier<SpoonTestRun>().Id;
		return run.Modifiers.Any(m => m.Id == id);
	}
}
