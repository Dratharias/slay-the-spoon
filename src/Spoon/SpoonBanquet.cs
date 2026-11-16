using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>Show the fixed HP loss, never the normal attack calculation with Strength/Appetite/Vulnerable.</summary>
internal sealed class SpoonBanquetIntent : SingleAttackIntent
{
	public SpoonBanquetIntent() : base(SpoonRule.BanquetDamage) { }

	public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
	{
		var player = LocalContext.GetMe(owner.CombatState);
		return player?.Creature.GetPower<SpoonSkimmedPower>() != null ? 0 : SpoonRule.BanquetDamage;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		var text = new LocString("powers", "SPOON_SKIMMED_POWER.banquetDescription");
		text.Add("Damage", GetTotalDamage(targets, owner));
		return text;
	}
}

/// <summary>
/// Banquet is fixed HP loss countered only by Skimmed. Keep the normal damage command for damage/death events,
/// but bypass scaling, prevention (including Buffer) and pet redirection for this one scoped damage call.
/// </summary>
[HarmonyPatch]
internal static class SpoonBanquetDamagePatch
{
	private static bool IsBanquet(Creature? target, Creature? dealer, ValueProp props) =>
		target != null && dealer?.Monster is SpoonMonster spoon && spoon.BanquetTarget == target &&
		props == (ValueProp.Unblockable | ValueProp.Unpowered);

	[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
	[HarmonyPrefix]
	private static bool FixedDamage(Creature? target, Creature? dealer, ValueProp props,
		ref IEnumerable<AbstractModel> modifiers, ref decimal __result)
	{
		if (!IsBanquet(target, dealer, props)) return true;
		modifiers = Array.Empty<AbstractModel>();
		__result = SpoonRule.BanquetDamage;
		return false;
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyHpLost))]
	[HarmonyPrefix]
	private static bool FixedLoss(Creature target, Creature? dealer, decimal amount, ValueProp props,
		ref IEnumerable<AbstractModel> modifiers, ref decimal __result)
	{
		if (!IsBanquet(target, dealer, props)) return true;
		modifiers = Array.Empty<AbstractModel>();
		__result = amount;
		return false;
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyUnblockedDamageTarget))]
	[HarmonyPrefix]
	private static bool KeepVictim(Creature originalTarget, Creature? dealer, ValueProp props, ref Creature __result)
	{
		if (!IsBanquet(originalTarget, dealer, props)) return true;
		__result = originalTarget;
		return false;
	}
}
