using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>Spoon mode rest option: gain max HP instead of resting.</summary>
public sealed class SpoonFeastRestSiteOption(Player owner) : RestSiteOption(owner)
{
	/// <summary>The heal option's icon.</summary>
	public static string IconImage => ImageHelper.GetImagePath("ui/rest_site/option_heal.png");

	public override string OptionId => "SPOON_FEAST";
	public override IEnumerable<string> AssetPaths => [IconImage];

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			description.Add("MaxHp", SpoonRule.FeastMaxHp);
			return description;
		}
	}

	public override async Task<bool> OnSelect()
	{
		await CreatureCmd.GainMaxHp(Owner.Creature, SpoonRule.FeastMaxHp);
		return true;
	}
}
