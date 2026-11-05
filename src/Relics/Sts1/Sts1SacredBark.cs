using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1SacredBark : Sts1InactiveBossRelic { }

/// <summary>
/// StS2 has no potion potency hook. Every numeric potion var is read from <see cref="PotionModel.DynamicVars"/>, for
/// both the text and the effect, so the getter scales the held potion's vars while its owner holds the bark (and back
/// when the bark leaves). Potions without a number (choose-a-card potions, Smoke Bomb) stay as they are, as in StS1.
/// </summary>
[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.DynamicVars), MethodType.Getter)]
public static class SacredBarkPotionPatch
{
	private static readonly AccessTools.FieldRef<PotionModel, Player?> OwnerField = AccessTools.FieldRefAccess<PotionModel, Player?>("_owner");
	private static readonly ConditionalWeakTable<PotionModel, StrongBox<int>> Applied = new();

	public static void Postfix(PotionModel __instance, DynamicVarSet __result)
	{
		if (!__instance.IsMutable) return;
		var owner = OwnerField(__instance);
		int want = Sts1RelicRule.PotionFactor(owner != null && owner.Relics.OfType<Sts1SacredBark>().Any());
		var applied = Applied.GetValue(__instance, _ => new StrongBox<int>(1));
		if (applied.Value == want) return;
		foreach (var v in __result.Values) v.BaseValue = v.BaseValue / applied.Value * want;
		applied.Value = want;
	}
}

/// <summary>
/// MemberwiseClone shares the canonical potion's var set with the mutable copy. Give each copy its own before the
/// bark scales it, so the canonical (compendium, shop) values never change.
/// </summary>
[HarmonyPatch(typeof(PotionModel), "AfterCloned")]
public static class SacredBarkClonePatch
{
	private static readonly AccessTools.FieldRef<PotionModel, DynamicVarSet?> Vars = AccessTools.FieldRefAccess<PotionModel, DynamicVarSet?>("_dynamicVars");
	public static void Postfix(PotionModel __instance) => Vars(__instance) = null;
}
