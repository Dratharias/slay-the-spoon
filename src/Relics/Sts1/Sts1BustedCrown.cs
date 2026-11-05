using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1BustedCrown : Sts1EnergyBossRelic
{
	protected override IEnumerable<DynamicVar> ExtraVars => new DynamicVar[] { new CardsVar(2) };
	public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
	{
		if (player != Owner) return false;
		int keep = Sts1RelicRule.BustedCrownOptions(options.Count, DynamicVars.Cards.IntValue);
		if (keep >= options.Count) return false;
		options.RemoveRange(keep, options.Count - keep);
		return true;
	}
}
