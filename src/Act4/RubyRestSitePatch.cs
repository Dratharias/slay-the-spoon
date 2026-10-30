using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

[HarmonyPatch(typeof(RestSiteOption), "get_IconPath")]
internal static class RubyRestSiteIconPatch
{
	private static void Postfix(RestSiteOption __instance, ref string __result)
	{
		if (__instance is Sts1RubyRestSiteOption)
			__result = KeySprites.Path(Act4Key.Ruby, large: true);
	}
}

[HarmonyPatch(typeof(NRestSiteRoom), "OnAfterPlayerSelectedRestSiteOption")]
internal static class RubyRestSitePatch
{
	private static readonly AccessTools.FieldRef<NRestSiteButton, bool> Unclickable =
		AccessTools.FieldRefAccess<NRestSiteButton, bool>("_isUnclickable");

	private static void Postfix(NRestSiteRoom __instance)
	{
		foreach (var option in __instance.Options.OfType<Sts1RubyRestSiteOption>())
		{
			if (option.IsEnabled || __instance.GetButtonForOption(option) is not { } button) continue;
			// Keep the option and its synchronized index. EnableOptions must not make it selectable again.
			Unclickable(button) = true;
			button.Option = option; // Refresh the disabled icon while preserving hover information.
		}
	}
}
