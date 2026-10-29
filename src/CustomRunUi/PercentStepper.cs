using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.CustomRunUi;

/// <summary>
/// The −/+ buttons on a percent modifier row. A press swaps the row's modifier for the family member
/// with the next percent and tells the list it changed, which makes the host re-send the modifiers.
/// The buttons take no focus: with a gamepad (or the arrow keys), left/right on the focused row step it,
/// like the game's settings sliders.
/// </summary>
public static class PercentStepper
{
	private const string ButtonsNodeName = "SpoonStepper";
	private const string RowNodeName = "HBoxContainer";

	private static readonly AccessTools.FieldRef<NRunModifierTickbox, MegaRichTextLabel> Label =
		AccessTools.FieldRefAccess<NRunModifierTickbox, MegaRichTextLabel>("_label");

	private static readonly MethodInfo ModifierSetter =
		AccessTools.PropertySetter(typeof(NRunModifierTickbox), nameof(NRunModifierTickbox.Modifier));

	public static void Attach(NRunModifierTickbox tickbox)
	{
		if (tickbox.Modifier is not SpoonPercentModifier)
		{
			return;
		}
		// The row is the scene's HBoxContainer (checkbox, label); the buttons go after the label.
		HBoxContainer buttons = new() { Name = ButtonsNodeName, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter };
		buttons.AddChild(MakeButton("−", tickbox, -1));
		buttons.AddChild(MakeButton("+", tickbox, 1));
		tickbox.GetNode<HBoxContainer>(RowNodeName).AddChild(buttons);
		tickbox.GuiInput += input => OnGuiInput(tickbox, input);
		SetButtonsEnabled(tickbox, tickbox.IsEnabled);
		RefreshLabel(tickbox);
	}

	/// <summary>Puts <paramref name="modifier"/> on the row and redraws its label.</summary>
	public static void ReplaceModifier(NRunModifierTickbox tickbox, ModifierModel modifier)
	{
		ModifierSetter.Invoke(tickbox, new object[] { modifier });
		RefreshLabel(tickbox);
	}

	/// <summary>Same text the game builds in NRunModifierTickbox._Ready, with the current percent.</summary>
	public static void RefreshLabel(NRunModifierTickbox tickbox)
	{
		ModifierModel modifier = tickbox.Modifier!;
		LocString text = new("main_menu_ui", "CUSTOM_RUN_SCREEN.MODIFIER_LABEL");
		text.Add("color", "blue");
		text.Add("modifier_title", modifier.Title.GetFormattedText());
		text.Add("modifier_description", modifier.Description.GetFormattedText());
		Label(tickbox).Text = text.GetFormattedText();
	}

	public static void SetButtonsEnabled(NRunModifierTickbox tickbox, bool enabled)
	{
		HBoxContainer? buttons = tickbox.GetNodeOrNull<HBoxContainer>($"{RowNodeName}/{ButtonsNodeName}");
		if (buttons == null)
		{
			return;
		}
		foreach (Button button in buttons.GetChildren().OfType<Button>())
		{
			button.Disabled = !enabled;
		}
	}

	private static Button MakeButton(string text, NRunModifierTickbox tickbox, int direction)
	{
		Button button = new()
		{
			Text = text,
			CustomMinimumSize = new Vector2(48, 48),
			FocusMode = Control.FocusModeEnum.None,
		};
		button.AddThemeFontSizeOverride("font_size", 28);
		button.AddThemeColorOverride("font_color", new Color(0.96f, 0.85f, 0.55f));
		button.AddThemeColorOverride("font_disabled_color", new Color(0.5f, 0.5f, 0.5f));
		foreach (string state in new[] { "normal", "hover", "pressed", "disabled" })
		{
			button.AddThemeStyleboxOverride(state, MakeStyle(state == "hover" ? 0.32f : 0.18f));
		}
		button.Pressed += () => OnPressed(tickbox, direction);
		return button;
	}

	private static StyleBoxFlat MakeStyle(float brightness)
	{
		StyleBoxFlat style = new()
		{
			BgColor = new Color(brightness, brightness * 0.9f, brightness * 0.75f, 0.9f),
			BorderColor = new Color(0.78f, 0.65f, 0.35f),
		};
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(6);
		return style;
	}

	/// <summary>Left/right on the focused row; the event is consumed so focus doesn't leave the list.</summary>
	private static void OnGuiInput(NRunModifierTickbox tickbox, InputEvent input)
	{
		int direction = input.IsActionPressed(MegaInput.left) ? -1 : input.IsActionPressed(MegaInput.right) ? 1 : 0;
		if (direction == 0 || !tickbox.IsEnabled)
		{
			return;
		}
		tickbox.AcceptEvent();
		OnPressed(tickbox, direction);
	}

	private static void OnPressed(NRunModifierTickbox tickbox, int direction)
	{
		if (!tickbox.IsEnabled || tickbox.Modifier is not SpoonPercentModifier current)
		{
			return;
		}
		int next = ScalingMath.Step(current.Percent, direction);
		if (next == current.Percent)
		{
			return;
		}
		ReplaceModifier(tickbox, PercentModifierTable.CanonicalFor(current.FamilyType, next).ToMutable());
		// Changing the value means the player wants the option: tick the row so it is actually sent.
		tickbox.IsTicked = true;
		FindList(tickbox)?.EmitSignal(NCustomRunModifiersList.SignalName.ModifiersChanged);
	}

	private static NCustomRunModifiersList? FindList(Node node)
	{
		for (Node? parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is NCustomRunModifiersList list)
			{
				return list;
			}
		}
		return null;
	}
}

[HarmonyPatch(typeof(NRunModifierTickbox), nameof(NRunModifierTickbox._Ready))]
public static class TickboxReadyPatch
{
	public static void Postfix(NRunModifierTickbox __instance)
	{
		PercentStepper.Attach(__instance);
	}
}

[HarmonyPatch(typeof(NRunModifierTickbox), "OnEnable")]
public static class TickboxEnablePatch
{
	public static void Postfix(NRunModifierTickbox __instance)
	{
		PercentStepper.SetButtonsEnabled(__instance, true);
	}
}

[HarmonyPatch(typeof(NRunModifierTickbox), "OnDisable")]
public static class TickboxDisablePatch
{
	public static void Postfix(NRunModifierTickbox __instance)
	{
		PercentStepper.SetButtonsEnabled(__instance, false);
	}
}
