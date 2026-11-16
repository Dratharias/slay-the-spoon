using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.CardSets;
using SlaySpoon.Core;
using SlaySpoon.Relics;

namespace SlaySpoon.Config;

/// <summary>
/// Card set mode for the run. One sealed class per mode (none ticked = StS2 only; Hybrid comes from the Spire Souvenir), so the mode travels as the
/// modifier id like the percents. The pools are swapped by <see cref="CardSets.UnlockedCardsPatch"/>.
/// </summary>
public abstract class SpoonCardSet : ModifierModel
{
	public abstract CardSetMode Mode { get; }
}

public sealed class SpoonCardSetSts1Only : SpoonCardSet
{
	public override CardSetMode Mode => CardSetMode.Sts1Only;
}

/// <summary>
/// Spire Souvenir run: every player starts with the Spire Souvenir, which puts them in Hybrid (only the colorless
/// cards for the Necrobinder and the Regent, owner 2026-11-16). The run itself stays StS2; the relic carries the effect, so losing it ends it. Runs on every peer at
/// run creation; the relic is saved with the player afterwards.
/// </summary>
public sealed class SpoonSpireSouvenirModifier : ModifierModel
{
	protected override void AfterRunCreated(RunState runState)
	{
		foreach (Player player in runState.Players)
		{
			if (!player.Relics.Any(r => r is SpoonSpireSouvenir))
			{
				player.AddRelicInternal(ModelDb.Relic<SpoonSpireSouvenir>().ToMutable());
			}
		}
	}
}
