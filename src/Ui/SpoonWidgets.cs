using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace SlaySpoon.Ui;

/// <summary>
/// The mod's panel widgets, usable with a controller: buttons answer the game's confirm (ui_select), and a value row
/// takes focus and steps with left/right like the Custom Run percent rows (ten times faster while held).
/// </summary>
public static class SpoonWidgets
{
	public static readonly Color Gold = new(0.96f, 0.85f, 0.55f);

	public static Button Button(Control parent, string text, Action action, bool disabled = false)
	{
		var button = new Button { Text = text, Disabled = disabled, FocusMode = Control.FocusModeEnum.All };
		button.AddThemeFontSizeOverride("font_size", 18);
		button.Pressed += action;
		// The game confirms with ui_select; Godot buttons only listen to ui_accept.
		button.GuiInput += input =>
		{
			if (!button.Disabled && input.IsActionPressed(MegaInput.select))
			{
				button.AcceptEvent();
				action();
			}
		};
		parent.AddChild(button);
		return button;
	}

	/// <summary>
	/// A value row: the focusable title (left/right step it by the smallest step, ten times that while held), then
	/// mouse buttons: reset (delta 0, when <paramref name="reset"/> is given), − buttons from the largest step and
	/// + buttons from the smallest. Returns the title label.
	/// </summary>
	public static Label Stepper(Control parent, string title, string? reset, Action<int> change, params int[] steps)
	{
		if (steps.Length == 0) steps = new[] { 1, 5, 10 };
		var row = new PanelContainer { FocusMode = Control.FocusModeEnum.All };
		row.AddThemeStyleboxOverride("panel", RowStyle(focused: false));
		row.FocusEntered += () => row.AddThemeStyleboxOverride("panel", RowStyle(focused: true));
		row.FocusExited += () => row.AddThemeStyleboxOverride("panel", RowStyle(focused: false));
		row.GuiInput += input =>
		{
			int direction = input.IsActionPressed(MegaInput.left, allowEcho: true) ? -1
				: input.IsActionPressed(MegaInput.right, allowEcho: true) ? 1 : 0;
			if (direction == 0) return;
			row.AcceptEvent();
			change(direction * steps[0] * (input.IsEcho() ? 10 : 1));
		};
		row.MouseEntered += () => row.GrabFocus();
		parent.AddChild(row);
		var column = new VBoxContainer();
		row.AddChild(column);
		var label = new Label { Text = "◀  " + title + "  ▶", HorizontalAlignment = HorizontalAlignment.Center };
		label.AddThemeFontSizeOverride("font_size", 20);
		label.AddThemeColorOverride("font_color", Gold);
		column.AddChild(label);
		var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		column.AddChild(buttons);
		var all = new List<Button>();
		if (reset != null) all.Add(Button(buttons, reset, () => change(0)));
		foreach (int step in steps.Reverse()) all.Add(Button(buttons, "−" + step, () => change(-step)));
		foreach (int step in steps) all.Add(Button(buttons, "+" + step, () => change(step)));
		// The row takes the controller; its buttons are for the mouse.
		foreach (var button in all) button.FocusMode = Control.FocusModeEnum.None;
		return label;
	}

	public static StyleBoxFlat RowStyle(bool focused)
	{
		var style = new StyleBoxFlat { BgColor = new Color(1, 1, 1, focused ? 0.08f : 0.03f),
			BorderColor = focused ? Gold : new Color(Gold, 0.25f),
			ContentMarginLeft = 8, ContentMarginRight = 8, ContentMarginTop = 6, ContentMarginBottom = 6 };
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(6);
		return style;
	}

	/// <summary>The dark panel the mod's popups use.</summary>
	public static StyleBoxFlat PanelStyle()
	{
		return new StyleBoxFlat { BgColor = new Color(0.08f, 0.07f, 0.06f, 0.97f),
			ContentMarginLeft = 14, ContentMarginRight = 14, ContentMarginTop = 12, ContentMarginBottom = 12 };
	}

	/// <summary>Controller-focusable controls under <paramref name="root"/>, in tree order.</summary>
	public static List<Control> Focusables(Node root)
	{
		var found = new List<Control>();
		void Walk(Node node)
		{
			foreach (Node child in node.GetChildren())
			{
				if (child is Control { FocusMode: Control.FocusModeEnum.All } control
					&& control.IsVisibleInTree() && control is not BaseButton { Disabled: true }
					&& control is not NClickableControl { IsEnabled: false })
					found.Add(control);
				Walk(child);
			}
		}
		Walk(root);
		return found;
	}

	/// <summary>
	/// After a rebuild: focus the control at the index that had focus before, or the first one when the panel just
	/// appeared and a controller is in use.
	/// </summary>
	public static void RestoreFocus(Node root, int focused, bool appeared)
	{
		var focusables = Focusables(root);
		if (focusables.Count == 0) return;
		if (focused >= 0) focusables[Math.Min(focused, focusables.Count - 1)].GrabFocus();
		else if (appeared && NControllerManager.Instance?.IsUsingDirectionalNavigation == true) focusables[0].GrabFocus();
	}
}
