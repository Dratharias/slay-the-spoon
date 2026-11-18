using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Campfire;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// The Ruby, Campfire Gift, King Midas Reopen, Spoon Feast and Forge Fuse options' icons. The private <c>IconPath</c> getter is small enough for the JIT to inline into <c>Icon</c>,
/// which would skip its postfix, so the public <c>Icon</c> getter is patched as well; either patch alone is enough.
/// </summary>
[HarmonyPatch(typeof(RestSiteOption), "get_IconPath")]
internal static class RubyRestSiteIconPatch
{
	private static void Postfix(RestSiteOption __instance, ref string __result)
	{
		if (__instance is Sts1RubyRestSiteOption)
			__result = KeySprites.Path(Act4Key.Ruby, large: true);
		else if (__instance is SpoonGiftRestSiteOption)
			__result = SpoonGiftRestSiteOption.IconImage;
		else if (__instance is Midas.MawBankRestSiteOption)
			__result = Midas.MawBankRestSiteOption.IconImage;
		else if (__instance is Spoon.SpoonFeastRestSiteOption)
			__result = Spoon.SpoonFeastRestSiteOption.IconImage;
		else if (__instance is Forge.ForgeFuseRestSiteOption)
			__result = Forge.ForgeFuseRestSiteOption.IconImage;
	}
}

[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Icon), MethodType.Getter)]
internal static class RubyRestSiteTexturePatch
{
	private static bool Prefix(RestSiteOption __instance, ref Texture2D __result)
	{
		string? path = __instance switch
		{
			Sts1RubyRestSiteOption => KeySprites.Path(Act4Key.Ruby, large: true),
			SpoonGiftRestSiteOption => SpoonGiftRestSiteOption.IconImage,
			Midas.MawBankRestSiteOption => Midas.MawBankRestSiteOption.IconImage,
			Spoon.SpoonFeastRestSiteOption => Spoon.SpoonFeastRestSiteOption.IconImage,
			Forge.ForgeFuseRestSiteOption => Forge.ForgeFuseRestSiteOption.IconImage,
			_ => null,
		};
		if (path == null) return true;
		__result = PreloadManager.Cache.GetTexture2D(path);
		return false;
	}
}

[HarmonyPatch(typeof(NRestSiteRoom), "OnAfterPlayerSelectedRestSiteOption")]
internal static class RubyRestSitePatch
{
	private static readonly AccessTools.FieldRef<NRestSiteButton, bool> Unclickable =
		AccessTools.FieldRefAccess<NRestSiteButton, bool>("_isUnclickable");

	private static void Postfix(NRestSiteRoom __instance)
	{
		foreach (var option in __instance.Options.Where(o => o is Sts1RubyRestSiteOption or SpoonGiftRestSiteOption))
		{
			if (option.IsEnabled || __instance.GetButtonForOption(option) is not { } button) continue;
			// Keep the option and its synchronized index. EnableOptions must not make it selectable again.
			Unclickable(button) = true;
			button.Option = option; // Refresh the disabled icon while preserving hover information.
		}
	}
}
