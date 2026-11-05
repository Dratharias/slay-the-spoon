using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1CoffeeDripper : Sts1EnergyBossRelic
{
	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != Owner) return false;
		foreach (var option in options.OfType<HealRestSiteOption>().ToList()) options.Remove(option);
		return true;
	}
}
