using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;

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

	public static void Prefix(OrbModel __instance)
	{
		Current = __instance;
	}

	public static Exception? Finalizer(Exception? __exception)
	{
		Current = null;
		return __exception;
	}
}
