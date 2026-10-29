using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.CardSets;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Relics;

/// <summary>
/// Spire Souvenir (spec 2026-10-29-spire-souvenir-relic-design.md): a Common Ironclad relic that puts its owner in
/// Hybrid. A Spire Souvenir run (<see cref="SpoonSpireSouvenirModifier"/>) hands it out at the start. It has no hooks; <see cref="CardSetPools.ModeFor"/> reads whether a player holds it.
/// </summary>
public sealed class SpoonSpireSouvenir : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	/// <summary>The mod ships no relic art: atlas icon, outline and big icon all come from Dusty Tome.</summary>
	protected override string IconBaseName => "dusty_tome";

	public override bool IsAllowed(IRunState runState)
	{
		return IsOffered(runState);
	}

	/// <summary>The relic may drop in <paramref name="run"/>: not in a Spire Souvenir run, nor in a Daily run.</summary>
	public static bool IsOffered(IRunState run)
	{
		return SouvenirRule.IsOffered(run.Modifiers.OfType<SpoonSpireSouvenirModifier>().Any(),
			run.GameMode == GameMode.Daily);
	}
}
