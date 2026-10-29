using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.CardSets;
using SlaySpoon.Core;

namespace SlaySpoon.Relics;

/// <summary>
/// Spire Souvenir (spec 2026-10-29-spire-souvenir-relic-design.md): a Common Ironclad relic that puts its owner in
/// Hybrid. It has no hooks; <see cref="CardSetPools.ModeFor"/> reads whether a player holds it.
/// </summary>
public sealed class SpoonSpireSouvenir : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	/// <summary>The mod ships no relic art: atlas icon, outline and big icon all come from Dusty Tome.</summary>
	protected override string IconBaseName => "dusty_tome";

	public override bool IsAllowed(IRunState runState)
	{
		return SouvenirRule.IsOffered(CardSetPools.ModeOf(runState), runState.GameMode == GameMode.Daily);
	}
}
