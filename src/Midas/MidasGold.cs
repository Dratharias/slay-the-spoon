using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Midas;

/// <summary>
/// King Midas gold melt: each time the team travels to a map room, every living player loses
/// <see cref="MidasRule.Melt"/> (10 %, at least their average gold gained per floor); a player the melt leaves with no gold dies. It runs inside the synced travel action,
/// on the map before the fade, so every peer melts the same synced gold and a death shows over the map (a death while
/// the screen is faded out would hide the game over screen). Act starts are not travels and do not melt.
/// </summary>
public static class MidasMelt
{
	public static async Task Apply(IRunState run)
	{
		var ruined = new List<Creature>();
		foreach (Player player in run.Players)
		{
			if (player.Creature.IsDead) continue;
			int gold = player.Gold;
			int floor = FloorFor(player);
			int melt = MidasRule.Melt(gold, floor);
			if (melt > 0) await PlayerCmd.LoseGold(melt, player, GoldLossType.Lost);
			if (MidasRule.Ruined(gold, floor))
			{
				SpoonLog.Info($"King Midas: player {player.NetId} ruined: {gold} gold, melt floor {floor}");
				ruined.Add(player.Creature);
			}
		}
		if (ruined.Count == 0) return;
		SpoonLog.Info($"King Midas: {ruined.Count} player(s) ruined by the melt");
		// Forced: ruin is not damage, so Lizard Tail and Fairy in a Bottle do not save the player.
		await CreatureCmd.Kill(ruined, force: true);
	}

	/// <summary>
	/// The melt's fixed part for <paramref name="player"/> (<see cref="MidasRule.MeltFloor"/>), from the run's floor
	/// history: the gold they gained on every floor so far, the floor being left included. Every peer records the same.
	/// </summary>
	public static int FloorFor(Player player)
	{
		int gained = 0, floors = 0;
		foreach (var act in player.RunState.MapPointHistory)
		{
			foreach (var entry in act)
			{
				floors++;
				gained += entry.PlayerStats.Where(p => p.PlayerId == player.NetId).Sum(p => p.GoldGained);
			}
		}
		return MidasRule.MeltFloor(gained, floors);
	}

	public static bool CanSpend(Player player, int price) => MidasRule.CanSpend(player.Gold, price, FloorFor(player));

	/// <summary>True when the run continues (someone is still alive).</summary>
	public static bool AnyoneAlive(IRunState run) => run.Players.Any(p => !p.Creature.IsDead);

	/// <summary>True when <paramref name="player"/> could pay <paramref name="price"/> but the next melt would kill them.</summary>
	public static bool Fatal(Player player, int price) =>
		SpoonRoiMidas.IsActive(player.RunState) && player.Gold >= price && !CanSpend(player, price);
}

[HarmonyPatch(typeof(MoveToMapCoordAction), "ExecuteAction")]
public static class MidasMeltTravelPatch
{
	private static readonly AccessTools.FieldRef<MoveToMapCoordAction, Player> PlayerOf =
		AccessTools.FieldRefAccess<MoveToMapCoordAction, Player>("_player");

	private static readonly AccessTools.FieldRef<MoveToMapCoordAction, MapCoord> Destination =
		AccessTools.FieldRefAccess<MoveToMapCoordAction, MapCoord>("_destination");

	public static bool Prefix(MoveToMapCoordAction __instance, ref Task __result)
	{
		IRunState run = PlayerOf(__instance).RunState;
		if (!SpoonRoiMidas.IsActive(run)) return true;
		__result = MeltThenTravel(run, Destination(__instance));
		return false;
	}

	// The vanilla body, after the melt.
	private static async Task MeltThenTravel(IRunState run, MapCoord destination)
	{
		await MidasMelt.Apply(run);
		if (!MidasMelt.AnyoneAlive(run)) return;
		if (TestMode.IsOn)
		{
			await RunManager.Instance.FadeOut();
			await RunManager.Instance.EnterMapCoord(destination);
			_ = TaskHelper.RunSafely(RunManager.Instance.FadeIn());
		}
		else
		{
			await NMapScreen.Instance!.TravelToMapCoord(destination);
		}
	}
}

/// <summary>The merchant refuses a sale that would leave the buyer ruined by the next melt (shown as too expensive).</summary>
[HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.OnTryPurchaseWrapper))]
public static class MidasMerchantRefusePatch
{
	internal static readonly AccessTools.FieldRef<MerchantEntry, Player> PlayerOf =
		AccessTools.FieldRefAccess<MerchantEntry, Player>("_player");

	public static bool Prefix(MerchantEntry __instance, bool ignoreCost, ref Task<bool> __result) =>
		MidasMerchantGuard.Allow(__instance, ignoreCost, ref __result);
}

[HarmonyPatch(typeof(MerchantCardRemovalEntry), nameof(MerchantCardRemovalEntry.OnTryPurchaseWrapper))]
public static class MidasRemovalRefusePatch
{
	public static bool Prefix(MerchantCardRemovalEntry __instance, bool ignoreCost, ref Task<bool> __result) =>
		MidasMerchantGuard.Allow(__instance, ignoreCost, ref __result);
}

/// <summary>Price labels turn red for a fatal purchase, like an unaffordable one.</summary>
[HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.EnoughGold), MethodType.Getter)]
public static class MidasEnoughGoldPatch
{
	public static void Postfix(MerchantEntry __instance, ref bool __result)
	{
		if (__result && MidasMelt.Fatal(MidasMerchantRefusePatch.PlayerOf(__instance), __instance.Cost)) __result = false;
	}
}

internal static class MidasMerchantGuard
{
	public static bool Allow(MerchantEntry entry, bool ignoreCost, ref Task<bool> result)
	{
		if (ignoreCost || !MidasMelt.Fatal(MidasMerchantRefusePatch.PlayerOf(entry), entry.Cost)) return true;
		entry.InvokePurchaseFailed(PurchaseStatus.FailureGold);
		result = Task.FromResult(false);
		return false;
	}
}
