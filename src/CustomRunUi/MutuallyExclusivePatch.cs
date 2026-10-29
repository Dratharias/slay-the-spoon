using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Config;

namespace SlaySpoon.CustomRunUi;

/// <summary>Ticking one card set mode in Custom Run unticks the other.</summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.MutuallyExclusiveModifiers), MethodType.Getter)]
public static class MutuallyExclusivePatch
{
	public static void Postfix(ref IReadOnlyList<IReadOnlySet<ModifierModel>> __result)
	{
		HashSet<ModifierModel> cardSets = new()
		{
			ModelDb.Modifier<SpoonCardSetSts1Only>(),
			ModelDb.Modifier<SpoonCardSetHybrid>(),
		};
		__result = __result.Append(cardSets).ToList();
	}
}
