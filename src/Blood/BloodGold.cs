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
///
/// The real <c>_gold</c> field mirrors the spendable HP: the runtime can inline the patched getter into a hot caller
/// (tiered compilation), which then reads the raw field. With the field at 0, <c>Player.ToSerializable</c> sent 0 gold and
/// the other peer's <see cref="Player.SyncWithSerializedPlayer"/> wrote it back as a purchase: each peer charged the other
/// player all their HP at the first combat (co-op state divergence). A state sync is never a payment either.
/// </summary>
public static class BloodGold
{
	private static readonly AccessTools.FieldRef<Player, int> GoldRef = AccessTools.FieldRefAccess<Player, int>("_gold");
	private static readonly AccessTools.FieldRef<Player, Action?> ChangedRef = AccessTools.FieldRefAccess<Player, Action?>("GoldChanged");

	private static bool Applies(Player player) => player.Creature != null && SpoonBlood.IsActive(player.RunState);

	[ThreadStatic] private static bool _syncing;

	/// <summary>Sets the real gold field to the spendable HP, what the patched getter answers.</summary>
	public static void Mirror(Player player)
	{
		if (player.Creature != null) GoldRef(player) = BloodRule.SpendableGold(player.Creature.CurrentHp);
	}

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
			if (_syncing)
			{
				Mirror(__instance);
				return false;
			}
			int hp = creature.CurrentHp;
			int paid = BloodRule.SpendableGold(hp) - value;
			int cost = paid > 0 ? BloodRule.HpCost(paid, hp) : 0;
			if (cost > 0)
			{
				creature.SetCurrentHpInternal(hp - cost);
				SpoonLog.Info($"blood: {__instance.NetId} paid {paid} gold with {cost} HP");
			}
			Mirror(__instance);
			return false;
		}
	}

	/// <summary>The top bar gold follows the HP.</summary>
	[HarmonyPatch(typeof(Creature), nameof(Creature.SetCurrentHpInternal))]
	public static class HpPatch
	{
		public static void Postfix(Creature __instance)
		{
			if (__instance.Player is not { } player || !Applies(player)) return;
			Mirror(player);
			ChangedRef(player)?.Invoke();
		}
	}

	/// <summary>Copying a peer's state (combat start) sets HP, then gold: never charge for it.</summary>
	[HarmonyPatch(typeof(Player), nameof(Player.SyncWithSerializedPlayer))]
	public static class SyncPatch
	{
		public static void Prefix() => _syncing = true;

		public static void Finalizer() => _syncing = false;
	}
}
