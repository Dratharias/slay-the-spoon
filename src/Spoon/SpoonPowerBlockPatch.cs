using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// A supporter's damage to the Spoon is nothing (spec §5). Hits with a dealer are zeroed in
/// <see cref="SpoonMonster.ModifyDamageMultiplicative"/>; the damage below has no supporter dealer, so it is stopped
/// where it starts: the damaging power is never applied to the Spoon. Pure debuffs (Weak, Vulnerable, Strength loss,
/// Lock-On...) still apply.
/// </summary>
[HarmonyPatch]
internal static class SpoonPowerBlockPatch
{
	/// <summary>
	/// Powers that damage or kill the creature that carries them. Vanilla: Poison, Doom and Demise (damage or kill with
	/// no dealer), Constrict, Strangle, Disintegration and Magic Bomb (the carrier deals the damage). The mod's: Corpse
	/// Explosion (deals the carrier's max HP on death) and Mark (Pressure Points' damage, no dealer).
	/// </summary>
	private static readonly HashSet<Type> Damaging =
	[
		typeof(PoisonPower), typeof(DoomPower), typeof(DemisePower), typeof(ConstrictPower), typeof(StranglePower),
		typeof(DisintegrationPower), typeof(MagicBombPower), typeof(Sts1CorpseExplosionPower), typeof(Sts1MarkPower),
	];

	/// <summary>The core apply (every PowerCmd.Apply overload ends here for a new power instance).</summary>
	[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply),
		[typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
	[HarmonyPrefix]
	private static bool BlockApply(PowerModel power, Creature target, decimal amount, Creature? applier, ref Task __result)
	{
		if (!Blocks(power, target, amount, applier)) return true;
		__result = Task.CompletedTask;
		return false;
	}

	/// <summary>Stacking onto a power the Spoon already has (applied by a fighter, say) goes through here instead.</summary>
	[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
	[HarmonyPrefix]
	private static bool BlockStack(PowerModel power, decimal offset, Creature? applier, ref Task<int> __result)
	{
		if (!Blocks(power, power.Owner, offset, applier)) return true;
		__result = Task.FromResult(0);
		return false;
	}

	/// <summary>
	/// Haunt is a buff on its player: each Soul they play damages a random enemy with no dealer. A supporter's Souls
	/// deal nothing to the Spoon (the only enemy), so the effect is skipped.
	/// </summary>
	[HarmonyPatch(typeof(HauntPower), nameof(HauntPower.AfterCardPlayed))]
	[HarmonyPrefix]
	private static bool SkipSupporterHaunt(HauntPower __instance, ref Task __result)
	{
		if (!SpoonFight.IsSupporter(__instance.Owner)) return true;
		__result = Task.CompletedTask;
		return false;
	}

	private static bool Blocks(PowerModel power, Creature? target, decimal amount, Creature? applier)
	{
		if (amount <= 0m || applier == null || target?.Monster is not SpoonMonster) return false;
		if (!Damaging.Contains(power.GetType()) || !SpoonFight.IsSupporter(applier)) return false;
		SpoonLog.Info($"spoon: blocked {power.GetType().Name} {amount} from a supporter on the Spoon");
		return true;
	}
}
