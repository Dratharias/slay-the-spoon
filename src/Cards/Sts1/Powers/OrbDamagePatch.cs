using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// Marks damage dealt by a Lightning or Dark orb, which reaches the damage hooks with no card
/// and no orb. Both orb methods run synchronously up to the damage hook of their one target,
/// so the marker is set on entry and cleared when the method hands back its task.
/// </summary>
[HarmonyPatch]
public static class OrbDamagePatch
{
	/// <summary>The orb whose damage is being computed, or null.</summary>
	[ThreadStatic] internal static OrbModel? Current;

	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AccessTools.Method(typeof(LightningOrb), "ApplyLightningDamage");
		yield return AccessTools.Method(typeof(DarkOrb), nameof(DarkOrb.Evoke));
	}

	public static void Prefix(OrbModel __instance, out OrbModel? __state)
	{
		__state = Current;
		Current = __instance;
	}

	public static Exception? Finalizer(Exception? __exception, OrbModel? __state)
	{
		Current = __state;
		return __exception;
	}
}

/// <summary>Only an Electrodynamics holder changes Lightning's targets; vanilla orbs stay untouched otherwise.</summary>
[HarmonyPatch(typeof(LightningOrb), "ApplyLightningDamage")]
public static class ElectrodynamicsPatch
{
	private static readonly Action<OrbModel> PlayEvokeSound = AccessTools.MethodDelegate<Action<OrbModel>>(
		AccessTools.Method(typeof(OrbModel), "PlayEvokeSfx"));

	public static bool Prefix(LightningOrb __instance, decimal value, PlayerChoiceContext choiceContext,
		ref Task<IEnumerable<Creature>> __result)
	{
		if (!__instance.Owner.Creature.HasPower<Sts1ElectrodynamicsPower>())
		{
			return true;
		}
		__result = DamageAll(__instance, value, choiceContext);
		return false;
	}

	private static async Task<IEnumerable<Creature>> DamageAll(LightningOrb orb, decimal value, PlayerChoiceContext context)
	{
		var targets = orb.CombatState.GetOpponentsOf(orb.Owner.Creature).Where(c => c.IsHittable).ToList();
		if (targets.Count == 0)
		{
			return targets;
		}
		PlayEvokeSound(orb);
		foreach (Creature target in targets)
		{
			if (!target.IsHittable)
			{
				continue;
			}
			VfxCmd.PlayOnCreature(target, "vfx/vfx_attack_lightning");
			// Damage modifiers run before Damage's first await. Scope the marker per target,
			// including targets reached after an asynchronous hit, without leaking it while waiting.
			OrbModel? previous = OrbDamagePatch.Current;
			Task<IEnumerable<DamageResult>> damage;
			try
			{
				OrbDamagePatch.Current = orb;
				damage = CreatureCmd.Damage(context, target, value, ValueProp.Unpowered, orb.Owner.Creature);
			}
			finally
			{
				OrbDamagePatch.Current = previous;
			}
			await damage;
		}
		return targets;
	}
}
