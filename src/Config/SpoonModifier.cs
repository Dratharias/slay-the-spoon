using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Random;
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

		// The Max HP relics join every grab bag (players in order, then the shared one) on one seeded Rng.
		var rng = new Rng(runState.Rng.Seed, "SlaySpoon.Spoon.Relics");
		foreach (Player player in runState.Players)
			SpoonRelics.AddTo(player.RelicGrabBag, rng);
		SpoonRelics.AddTo(runState.SharedRelicGrabBag, rng);
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		options.Add(new SpoonFeastRestSiteOption(player));
		return true;
	}
}
