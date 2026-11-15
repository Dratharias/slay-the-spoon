using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Spoon;

namespace SlaySpoon.Config;

/// <summary>
/// Spoon (Custom Run, docs/superpowers/specs/2026-11-14-spoon-design.md): grow your max HP with Spoonful; at 300 the Spoon
/// awaits after act 3. Later tasks add the relics, the merchant, the Feast and the fight.
/// </summary>
public sealed class SpoonSpoon : ModifierModel
{
	public static bool IsActive(IRunState? run) => run != null && run.Modifiers.Any(m => m is SpoonSpoon);

	public static SpoonSpoon? Of(IRunState? run) => run?.Modifiers.OfType<SpoonSpoon>().FirstOrDefault();

	// Every player starts with one Spoonful (Eternal: it can be neither removed nor transformed).
	protected override void AfterRunCreated(RunState runState)
	{
		foreach (Player player in runState.Players)
			player.Deck.AddInternal(runState.CreateCard<SpoonSpoonful>(player));
	}
}
