using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Combat;

/// <summary>
/// Scales enemy HP once, when the creature is created: after the game's HP roll and co-op scaling,
/// for initial enemies and mid-combat summons alike.
/// </summary>
[HarmonyPatch(typeof(CombatState), nameof(CombatState.CreateCreature))]
public static class EnemyHpPatch
{
	public static void Postfix(CombatState __instance, Creature __result)
	{
		if (__result.Side != CombatSide.Enemy)
		{
			return;
		}
		SpoonMonsterHp? modifier = __instance.RunState.Modifiers.OfType<SpoonMonsterHp>().FirstOrDefault();
		if (modifier == null)
		{
			return;
		}
		__result.SetMaxHpInternal(ScalingMath.ScaleHp(__result.MaxHp, modifier.Percent));
		__result.SetCurrentHpInternal(__result.MaxHp);
	}
}
