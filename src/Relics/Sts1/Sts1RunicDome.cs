using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1RunicDome : Sts1EnergyBossRelic { }

/// <summary>Intents are drawn per client: hide them on the screen of a holder only. Co-op allies still see them.</summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
public static class RunicDomeIntentPatch
{
	private static readonly HashSet<NCreature> Hidden = new();

	public static void Postfix(NCreature __instance)
	{
		var creature = __instance.Entity;
		if (creature == null || !creature.IsMonster || creature.CombatState == null) return;
		bool hide = LocalContext.GetMe(creature.CombatState)?.Relics.OfType<Sts1RunicDome>().Any() == true;
		if (hide)
		{
			Hidden.Add(__instance);
			__instance.IntentContainer.Visible = false;
		}
		else if (Hidden.Remove(__instance))
		{
			__instance.IntentContainer.Visible = true;
		}
		Hidden.RemoveWhere(n => !Godot.GodotObject.IsInstanceValid(n));
	}
}
