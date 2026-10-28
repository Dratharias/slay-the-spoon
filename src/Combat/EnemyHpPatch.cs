using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Combat;

/// <summary>
/// Enemy HP goes through Creature.ScaleHpForMultiplayer in co-op creation and in every mid-combat
/// HP reset (Test Subject's revived forms, Tough Egg hatchlings, Decimillipede bounds), so the
/// percent is applied there. Solo creation skips that helper, hence the CreateCreature patch.
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.ScaleHpForMultiplayer))]
public static class EnemyHpScalingPatch
{
	private static readonly Func<RunManager, RunState?> RunState =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	public static void Postfix(ref decimal __result)
	{
		RunState? state = RunState(RunManager.Instance);
		SpoonMonsterHp? modifier = state?.Modifiers.OfType<SpoonMonsterHp>().FirstOrDefault();
		if (modifier != null)
		{
			__result = ScalingMath.ScaleHp(__result, modifier.Percent);
		}
	}
}

/// <summary>Solo only: co-op creation is already scaled through ScaleHpForMultiplayer.</summary>
[HarmonyPatch(typeof(CombatState), nameof(CombatState.CreateCreature))]
public static class EnemyHpPatch
{
	public static void Postfix(CombatState __instance, Creature __result)
	{
		if (__result.Side != CombatSide.Enemy || __instance.Players.Count != 1)
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
