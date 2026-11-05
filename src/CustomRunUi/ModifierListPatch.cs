using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.CustomRunUi;

/// <summary>
/// Adds one row per Slay the Spoon family (and one per card set mode, plus Neow's Blessings, Merchant Loans, Campfire Gifts, and the Act 4 flag) to the Custom Run list only. ModelDb.GoodModifiers is left
/// alone because Daily runs draw from it: this list is the Custom Run screen's only.
/// </summary>
[HarmonyPatch(typeof(NCustomRunModifiersList), "GetAllModifiers")]
public static class ModifierListPatch
{
	public static void Postfix(ref IEnumerable<ModifierModel> __result)
	{
		ModifierModel[] ours =
		{
			PercentModifierTable.CanonicalFor(typeof(SpoonMonsterHp), ScalingMath.DefaultPercent).ToMutable(),
			PercentModifierTable.CanonicalFor(typeof(SpoonMonsterDamage), ScalingMath.DefaultPercent).ToMutable(),
			ModelDb.Modifier<SpoonCardSetSts1Only>().ToMutable(),
			ModelDb.Modifier<SpoonSpireSouvenirModifier>().ToMutable(),
			ModelDb.Modifier<SpoonNeowBlessing>().ToMutable(),
			ModelDb.Modifier<SpoonMerchantLoans>().ToMutable(),
			ModelDb.Modifier<SpoonCampfireGift>().ToMutable(),
			ModelDb.Modifier<SpoonAct4>().ToMutable(),
		};
		__result = __result.Concat(ours);
	}
}
