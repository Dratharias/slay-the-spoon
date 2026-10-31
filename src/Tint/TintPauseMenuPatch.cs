using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Localization;

namespace SlaySpoon.Tint;

/// <summary>Adds a "Character Color" button under the pause menu's Compendium (a copy of that button).</summary>
[HarmonyPatch(typeof(NPauseMenu), nameof(NPauseMenu._Ready))]
internal static class TintPauseMenuPatch
{
	private static void Postfix(NPauseMenu __instance)
	{
		try
		{
			var container = __instance.GetNode<Control>("%ButtonContainer");
			var compendium = container.GetNode<NPauseMenuButton>("Compendium");
			var button = (NPauseMenuButton)compendium.Duplicate();
			button.Name = "SpoonCharacterColor";
			container.AddChild(button);
			container.MoveChild(button, compendium.GetIndex() + 1);
			button.Visible = true;
			button.GetNode<MegaLabel>("Label").SetTextAutoSize(
				new LocString("main_menu_ui", "SPOON_TINT.button").GetFormattedText());
			button.Connect(NClickableControl.SignalName.Released,
				Callable.From<NButton>(_ => TintPanel.Open(__instance, button)));

			var buttons = container.GetChildren().OfType<NPauseMenuButton>().ToArray();
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].FocusNeighborTop = (i > 0 ? buttons[i - 1] : buttons[^1]).GetPath();
				buttons[i].FocusNeighborBottom = (i < buttons.Length - 1 ? buttons[i + 1] : buttons[0]).GetPath();
			}
		}
		catch (Exception e)
		{
			SpoonLog.Error($"Character color pause button failed: {e.Message}");
		}
	}
}
