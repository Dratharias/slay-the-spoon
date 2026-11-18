using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SlaySpoon.Config;

/// <summary>
/// Conductor (Custom Run, co-op, docs/superpowers/specs/2026-11-18-backlog-modes-design.md): after each combat won, one
/// player hands out every player's rewards (<see cref="Conductor.ConductorRewards"/>).
/// </summary>
public sealed class SpoonConductor : ModifierModel
{
	private int _previous = -1;

	public static SpoonConductor? Of(IRunState? run) =>
		run != null && run.Players.Count >= 2 ? run.Modifiers.OfType<SpoonConductor>().FirstOrDefault() : null;

	/// <summary>The last Conductor's slot, −1 before the first.</summary>
	[SavedProperty]
	public int SpoonConductorPrevious
	{
		get => _previous;
		set { AssertMutable(); _previous = value; }
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_previous = -1;
	}
}
