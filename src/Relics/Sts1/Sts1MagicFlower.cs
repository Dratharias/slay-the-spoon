using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1MagicFlower : Sts1Relic { }

// There is no general heal-modification hook. Patch the command once, before its HP and VFX calculations.
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Heal))]
public static class MagicFlowerHealPatch
{
	public static void Prefix(Creature creature, ref decimal amount)
	{
		if (amount <= 0 || !CombatManager.Instance.IsInProgress || creature.IsDead) return;
		var relic = creature.Player?.Relics.OfType<Sts1MagicFlower>().FirstOrDefault();
		if (relic == null) return;
		amount = Sts1RelicRule.CombatHeal(amount);
		relic.Flash();
	}
}
