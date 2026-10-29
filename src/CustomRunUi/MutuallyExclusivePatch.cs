using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Config;

namespace SlaySpoon.CustomRunUi;

/// <summary>Ticking "Slay the Spire 1 Cards" in Custom Run unticks "Spire Souvenir", and the other way round.</summary>
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
		__result = __result.Append(cardSets).ToList();
	}
}
