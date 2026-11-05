using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Campfire;

namespace SlaySpoon.Config;

/// <summary>
/// Campfire Gifts (Custom Run, co-op only): every rest site offers <see cref="SpoonGiftRestSiteOption"/>. Solo runs see
/// no option; the modifier holds no state.
/// </summary>
public sealed class SpoonCampfireGift : ModifierModel
{
	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player.RunState.Players.Count < 2) return false;
		options.Add(new SpoonGiftRestSiteOption(player));
		return true;
	}
}
