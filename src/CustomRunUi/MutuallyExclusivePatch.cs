using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.CustomRunUi;

/// <summary>Ticking "Slay the Spire 1 Cards" in Custom Run unticks "Spire Souvenir", and the other way round; King Midas and Matriochka likewise; Blood and either Merchant Loans or King Midas.</summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.MutuallyExclusiveModifiers), MethodType.Getter)]
public static class MutuallyExclusivePatch
{
	public static void Postfix(ref IReadOnlyList<IReadOnlySet<ModifierModel>> __result)
	{
		HashSet<ModifierModel> cardSets = new()
		{
			ModelDb.Modifier<SpoonCardSetSts1Only>(),
			ModelDb.Modifier<SpoonSpireSouvenirModifier>(),
		};
		// King Midas hands out no card reward; Matriochka one on every floor. Every Matriochka stepper value is its own type.
		HashSet<ModifierModel> midas = new() { ModelDb.Modifier<SpoonRoiMidas>() };
		for (int r = 0; r <= MatriochkaRule.MaxRerolls; r++)
			midas.Add(PercentModifierTable.CanonicalFor(typeof(SpoonMatriochka), r));
		// Blood erases gold: Merchant Loans and King Midas are gold economies.
		HashSet<ModifierModel> bloodLoans = new() { ModelDb.Modifier<SpoonBlood>(), ModelDb.Modifier<SpoonMerchantLoans>() };
		HashSet<ModifierModel> bloodMidas = new() { ModelDb.Modifier<SpoonBlood>(), ModelDb.Modifier<SpoonRoiMidas>() };
		__result = __result.Append(cardSets).Append(midas).Append(bloodLoans).Append(bloodMidas).ToList();
	}
}
