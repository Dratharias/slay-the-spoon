using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace SlaySpoon.Relics.Events;

/// <summary>StS1 Mark of the Bloom (Mind Bloom, I am Awake): its holder can no longer heal.</summary>
public sealed class Sts1MarkOfTheBloom : Sts1EventRelic
{
}

/// <summary>
/// Every heal goes through <see cref="CreatureCmd.Heal"/> (rest sites, potions, max HP gains), so one prefix blocks
/// them. A dead holder still comes back in co-op (builder call): the resurrection heal is let through.
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Heal))]
public static class Sts1MarkOfTheBloomPatch
{
	public static bool Prefix(Creature creature, ref Task __result)
	{
		if (creature?.Player is not { } player || creature.IsDead) return true;
		if (player.GetRelic<Sts1MarkOfTheBloom>() is not { } mark) return true;
		mark.Flash();
		__result = Task.CompletedTask;
		return false;
	}
}
