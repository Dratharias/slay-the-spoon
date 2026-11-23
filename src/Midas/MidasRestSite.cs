using System.Collections;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Midas;

/// <summary>
/// King Midas rest sites: every option costs gold (<see cref="MidasRule.RestPrice"/>, rising with each action this
/// player paid for at this fire) and the options come back after each one, so a player may act again while they can pay
/// and keep enough gold to survive the next melt (<see cref="MidasMelt.CanSpend"/>).
/// </summary>
public static class MidasRestSite
{
	public static int? PriceFor(Player player) =>
		SpoonRoiMidas.Of(player.RunState) is { } midas ? MidasRule.RestPrice(midas.PaidHere(player), SpoonRoiMidas.Scale(player.RunState)) : null;

	public static bool CannotAfford(RestSiteOption option) =>
		OwnerOf(option) is { } owner && PriceFor(owner) is { } price && !MidasMelt.CanSpend(owner, price);

	public static Player? OwnerOf(RestSiteOption option) => OptionOwner(option);

	private static readonly Func<RestSiteOption, Player> OptionOwner =
		AccessTools.MethodDelegate<Func<RestSiteOption, Player>>(AccessTools.PropertyGetter(typeof(RestSiteOption), "Owner"));

	public static string PriceLine(int price)
	{
		var line = new LocString("rest_site_ui", "SPOON_MIDAS.price");
		line.Add("Gold", price);
		return line.GetFormattedText();
	}
}

[HarmonyPatch(typeof(RestSiteSynchronizer), "ChooseOption")]
public static class MidasRestChoosePatch
{
	private static readonly System.Reflection.FieldInfo RestSitesField = AccessTools.Field(typeof(RestSiteSynchronizer), "_restSites");

	private static readonly AccessTools.FieldRef<RestSiteSynchronizer, IPlayerCollection> Players =
		AccessTools.FieldRefAccess<RestSiteSynchronizer, IPlayerCollection>("_playerCollection");

	private static readonly System.Reflection.FieldInfo OptionsField =
		AccessTools.Field(AccessTools.Inner(typeof(RestSiteSynchronizer), "PlayerRestSite"), "options");

	private static readonly System.Reflection.FieldInfo CompletionField =
		AccessTools.Field(AccessTools.Inner(typeof(RestSiteSynchronizer), "PlayerRestSite"), "completionTaskSource");

	// Runs on every peer with the same synced gold, so a refusal is the same everywhere.
	public static bool Prefix(Player player, ref Task<bool> __result, out int __state)
	{
		__state = MidasRestSite.PriceFor(player) ?? -1;
		if (__state < 0 || MidasMelt.CanSpend(player, __state)) return true;
		__result = Task.FromResult(false);
		__state = -1;
		return false;
	}

	public static void Postfix(RestSiteSynchronizer __instance, Player player, ref Task<bool> __result, int __state)
	{
		if (__state < 0) return;
		__result = PayAndRepeat(__result, __instance, player, __state);
	}

	private static async Task<bool> PayAndRepeat(Task<bool> choice, RestSiteSynchronizer sync, Player player, int price)
	{
		if (!await choice) return false;
		await PlayerCmd.LoseGold(price, player, GoldLossType.Spent);
		SpoonRoiMidas.Of(player.RunState)?.Paid(player);
		var sites = (IList)RestSitesField.GetValue(sync)!;
		object site = sites[Players(sync).GetPlayerSlotIndex(player)]!;
		OptionsField.SetValue(site, RestSiteOption.Generate(player));
		// The options are back, so the site is open again. A completed source would make the next choice throw
		// ("rest site has already been completed") and the room exit complete it twice (crash on travel).
		if (CompletionField.GetValue(site) is TaskCompletionSource { Task.IsCompleted: true })
			CompletionField.SetValue(site, new TaskCompletionSource());
		return true;
	}
}

/// <summary>Greys out what the player cannot pay for.</summary>
[HarmonyPatch(typeof(NRestSiteButton), nameof(NRestSiteButton.Create))]
public static class MidasRestButtonCreatePatch
{
	private static readonly AccessTools.FieldRef<NRestSiteButton, bool> Unclickable =
		AccessTools.FieldRefAccess<NRestSiteButton, bool>("_isUnclickable");

	public static void Postfix(RestSiteOption option, NRestSiteButton __result)
	{
		if (MidasRestSite.CannotAfford(option)) Unclickable(__result) = true;
	}
}

[HarmonyPatch(typeof(NRestSiteButton), "Reload")]
public static class MidasRestButtonReloadPatch
{
	private static readonly AccessTools.FieldRef<NRestSiteButton, ShaderMaterial> Hsv =
		AccessTools.FieldRefAccess<NRestSiteButton, ShaderMaterial>("_hsv");

	public static void Postfix(NRestSiteButton __instance)
	{
		if (!__instance.IsNodeReady() || Hsv(__instance) is not { } hsv) return;
		RestSiteOption option = __instance.Option;
		if (!MidasRestSite.CannotAfford(option)) return;
		hsv.SetShaderParameter("s", 0f);
		hsv.SetShaderParameter("v", 0.6f);
	}
}

/// <summary>Adds the price under the focused option's description.</summary>
[HarmonyPatch(typeof(NRestSiteButton), nameof(NRestSiteButton.RefreshTextState))]
public static class MidasRestButtonTextPatch
{
	private static readonly Func<NClickableControl, bool> Focused =
		AccessTools.MethodDelegate<Func<NClickableControl, bool>>(AccessTools.PropertyGetter(typeof(NClickableControl), "IsFocused"));

	public static void Postfix(NRestSiteButton __instance)
	{
		if (!Focused(__instance) || NRestSiteRoom.Instance is not { } room) return;
		RestSiteOption option = __instance.Option;
		if (MidasRestSite.OwnerOf(option) is not { } owner || MidasRestSite.PriceFor(owner) is not { } price) return;
		room.SetText(option.Description.GetFormattedText() + "\n" + MidasRestSite.PriceLine(price));
	}
}

/// <summary>
/// Vanilla enables the rest site's Proceed button only after an option was chosen. With every option greyed out (the
/// player cannot pay for any), nothing could be chosen and the room could not be left: Proceed opens at once then.
/// </summary>
[HarmonyPatch(typeof(NRestSiteRoom), "UpdateRestSiteOptions")]
public static class MidasRestLeavePatch
{
	private static readonly Action<NRestSiteRoom> ShowProceed =
		AccessTools.MethodDelegate<Action<NRestSiteRoom>>(AccessTools.Method(typeof(NRestSiteRoom), "ShowProceedButton"));

	public static void Postfix(NRestSiteRoom __instance)
	{
		try
		{
			if (__instance.Options.Count == 0 || SpoonRoiMidas.Of(MidasRestSite.OwnerOf(__instance.Options[0])?.RunState) is null) return;
			if (!MidasRule.LeaveOnly(__instance.Options.Select(o => !MidasRestSite.CannotAfford(o)))) return;
			SpoonLog.Info("King Midas: no rest site option affordable, Proceed enabled");
			ShowProceed(__instance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"King Midas: rest site leave check failed: {e.Message}");
		}
	}
}
