using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlaySpoon.Config;

namespace SlaySpoon.CustomRunUi;

/// <summary>
/// The game only re-ticks rows on clients; this also copies the host's percent onto our rows.
/// </summary>
[HarmonyPatch(typeof(NCustomRunModifiersList), nameof(NCustomRunModifiersList.SyncModifierList))]
public static class SyncModifierListPatch
{
	private static readonly AccessTools.FieldRef<NCustomRunModifiersList, List<NRunModifierTickbox>> Tickboxes =
		AccessTools.FieldRefAccess<NCustomRunModifiersList, List<NRunModifierTickbox>>("_modifierTickboxes");

	public static void Postfix(NCustomRunModifiersList __instance, IReadOnlyList<ModifierModel> modifiers)
	{
		foreach (NRunModifierTickbox tickbox in Tickboxes(__instance))
		{
			if (tickbox.Modifier is not SpoonPercentModifier mine)
			{
				continue;
			}
			SpoonPercentModifier? synced = modifiers.OfType<SpoonPercentModifier>()
				.FirstOrDefault(m => m.FamilyType == mine.FamilyType);
			if (synced != null && synced.GetType() != mine.GetType())
			{
				PercentStepper.ReplaceModifier(tickbox, PercentModifierTable.CanonicalFor(synced.FamilyType, synced.Percent).ToMutable());
			}
		}
	}
}
