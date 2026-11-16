using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using SlaySpoon.Config;

namespace SlaySpoon.Spoon;

/// <summary>
/// Fighters and supporters of the Spoon fight (spec §5). A fighter is a player who had 300+ max HP when the fight
/// started; every other player is a supporter. A summon (Osty) counts as its owner. Outside a Spoon combat nobody is
/// either.
/// </summary>
public static class SpoonFight
{
	/// <summary>The Spoon of this combat, when it is a Spoon combat of a Spoon run.</summary>
	public static SpoonMonster? Find(ICombatState? state)
	{
		if (state == null || !SpoonSpoon.IsActive(state.RunState)) return null;
		foreach (Creature enemy in state.Enemies)
		{
			if (enemy.Monster is SpoonMonster spoon) return spoon;
		}
		return null;
	}

	/// <summary>The player a creature belongs to: the player itself, or a summon's owner (null for monsters).</summary>
	public static Player? OwnerOf(Creature creature) => creature.Player ?? creature.PetOwner;

	public static bool IsFighter(Creature creature) =>
		Find(creature.CombatState) is { } spoon && OwnerOf(creature) is { } owner && spoon.IsFighter(owner);

	public static bool IsSupporter(Creature creature) =>
		Find(creature.CombatState) is { } spoon && OwnerOf(creature) is { } owner && !spoon.IsFighter(owner);

	/// <summary>The fighters' player creatures, dead ones included (empty outside a Spoon combat).</summary>
	public static IReadOnlyList<Creature> Fighters(ICombatState? state) =>
		Find(state) is { } spoon ? state!.Players.Where(spoon.IsFighter).Select(p => p.Creature).ToList() : [];
}

/// <summary>
/// A monster attack targets every player (AttackCommand.FromMonster, re-read at each hit). The Spoon's targets are the
/// fighters only, so a supporter is never hit: a 0-damage hit would still run their on-hit effects (Thorns and the like).
/// </summary>
[HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
internal static class SpoonAttackTargetsPatch
{
	[HarmonyPostfix]
	private static void FightersOnly(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
	{
		if (__instance.Attacker is not { Monster: SpoonMonster } attacker || __result.Count == 0) return;
		if (SpoonFight.Find(attacker.CombatState) == null) return;
		if (__result.All(SpoonFight.IsFighter)) return;
		__result = __result.Where(SpoonFight.IsFighter).ToList();
	}
}
