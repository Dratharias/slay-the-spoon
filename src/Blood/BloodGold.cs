using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Blood;

/// <summary>
/// Blood's gold: the game reads a Blood player's gold as their spendable HP (<see cref="BloodRule.SpendableGold"/>), and
/// any lower value written to it is paid in HP (<see cref="BloodRule.HpCost"/>). Gains are ignored. So every purchase
/// check and payment (merchant, events, campfire gifts and sales, Forge duplicate) works unchanged.
/// </summary>
public static class BloodGold
{
	private static readonly AccessTools.FieldRef<Player, int> GoldRef = AccessTools.FieldRefAccess<Player, int>("_gold");
	private static readonly AccessTools.FieldRef<Player, Action?> ChangedRef = AccessTools.FieldRefAccess<Player, Action?>("GoldChanged");

	private static bool Applies(Player player) => player.Creature != null && SpoonBlood.IsActive(player.RunState);

	public static void Clear(Player player) => GoldRef(player) = 0;

	[HarmonyPatch(typeof(Player), nameof(Player.Gold), MethodType.Getter)]
	public static class ReadPatch
	{
		public static void Postfix(Player __instance, ref int __result)
		{
			if (Applies(__instance)) __result = BloodRule.SpendableGold(__instance.Creature.CurrentHp);
		}
	}

	[HarmonyPatch(typeof(Player), nameof(Player.Gold), MethodType.Setter)]
	public static class WritePatch
	{
		public static bool Prefix(Player __instance, int value)
		{
			if (!Applies(__instance)) return true;
			var creature = __instance.Creature;
			int hp = creature.CurrentHp;
			int paid = BloodRule.SpendableGold(hp) - value;
			int cost = paid > 0 ? BloodRule.HpCost(paid, hp) : 0;
			if (cost > 0)
			{
				creature.SetCurrentHpInternal(hp - cost);
				SpoonLog.Info($"blood: {__instance.NetId} paid {paid} gold with {cost} HP");
			}
			GoldRef(__instance) = 0;
			return false;
		}
	}

	/// <summary>The top bar gold follows the HP.</summary>
	[HarmonyPatch(typeof(Creature), nameof(Creature.SetCurrentHpInternal))]
	public static class HpPatch
	{
		public static void Postfix(Creature __instance)
		{
			if (__instance.Player is { } player && Applies(player)) ChangedRef(player)?.Invoke();
		}
	}
}
