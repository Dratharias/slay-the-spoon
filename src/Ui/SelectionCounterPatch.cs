using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.addons.mega_text;
using SlaySpoon.Core;

namespace SlaySpoon.Ui;

/// <summary>
/// Adds "Selected: X / Y" under the prompt of every multi-pick card grid (owner decision 2026-11-05, vanilla modes
/// included, display only). Each grid screen keeps its own private selection set, prefs and prompt label, read here by
/// reflection. The line refreshes once per frame while the screen is in the tree, so every path that changes the
/// selection (clicks, cancelled previews, pile changes in combat) is covered without patching each one.
/// </summary>
[HarmonyPatch]
internal static class SelectionCounterPatch
{
	private static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
		new[]
		{
			typeof(NSimpleCardSelectScreen), typeof(NDeckCardSelectScreen), typeof(NDeckUpgradeSelectScreen),
			typeof(NDeckTransformSelectScreen), typeof(NDeckEnchantSelectScreen), typeof(NCombatPileCardSelectScreen),
		}.Select(t => AccessTools.Method(t, "_Ready"));

	private static void Postfix(NCardGridSelectionScreen __instance)
	{
		try
		{
			Type type = __instance.GetType();
			if (AccessTools.Field(type, "_selectedCards")?.GetValue(__instance) is not HashSet<CardModel> selected
			    || AccessTools.Field(type, "_prefs")?.GetValue(__instance) is not CardSelectorPrefs prefs
			    || AccessTools.Field(type, "_infoLabel")?.GetValue(__instance) is not MegaRichTextLabel label
			    || AccessTools.Field(typeof(NCardGridSelectionScreen), "_cards")?.GetValue(__instance) is not IReadOnlyList<CardModel> cards)
				return;
			if (SelectionCounter.Limit(prefs.MaxSelect, cards.Count) is not { } limit) return;

			string prompt = label.Text;
			int shown = -1;
			SceneTree tree = __instance.GetTree();
			Action refresh = null!;
			refresh = () =>
			{
				if (!GodotObject.IsInstanceValid(label) || !GodotObject.IsInstanceValid(__instance))
				{
					tree.ProcessFrame -= refresh;
					return;
				}
				if (selected.Count == shown) return;
				shown = selected.Count;
				var counter = new LocString("card_selection", "SPOON_SELECTED_COUNT");
				counter.Add("Selected", shown);
				counter.Add("Max", limit);
				label.Text = SelectionCounter.Compose(prompt, counter.GetFormattedText());
			};
			refresh();
			// Unsubscribes itself once the screen is freed.
			tree.ProcessFrame += refresh;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"ui: selection counter failed: {e}");
		}
	}
}
