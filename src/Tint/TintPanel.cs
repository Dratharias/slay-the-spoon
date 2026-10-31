using Godot;
using MegaCrit.Sts2.Core.Localization;
using SlaySpoon.Core;
using SlaySpoon.Ui;

namespace SlaySpoon.Tint;

/// <summary>
/// The pause menu's character color settings, usable with a controller. Changes show at once on this machine; they
/// are saved and shared with co-op partners when the panel closes.
/// </summary>
internal sealed class TintPanel
{
	private static LocString Loc(string key) => new("main_menu_ui", "SPOON_TINT." + key);
	private static string Text(string key) => Loc(key).GetFormattedText();

	private readonly Control _overlay;
	private readonly VBoxContainer _rows;
	private readonly Control _returnFocus;
	private CharacterTint _tint;

	private TintPanel(Control host, Control returnFocus)
	{
		_returnFocus = returnFocus;
		_tint = CharacterTints.Local;
		_overlay = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
		_overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		var dim = new ColorRect { Color = new Color(0, 0, 0, 0.5f), MouseFilter = Control.MouseFilterEnum.Ignore };
		dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_overlay.AddChild(dim);
		var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
		center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_overlay.AddChild(center);
		var panel = new PanelContainer { CustomMinimumSize = new Vector2(560, 0) };
		panel.AddThemeStyleboxOverride("panel", SpoonWidgets.PanelStyle());
		center.AddChild(panel);
		_rows = new VBoxContainer();
		_rows.AddThemeConstantOverride("separation", 8);
		panel.AddChild(_rows);
		_overlay.GuiInput += input =>
		{
			if (input.IsActionPressed("ui_cancel"))
			{
				_overlay.AcceptEvent();
				Close();
			}
		};
		host.AddChild(_overlay);
		Build(-1, appeared: true);
	}

	public static void Open(Control host, Control returnFocus) => new TintPanel(host, returnFocus);

	private void Change(CharacterTint tint)
	{
		_tint = tint.Clamped();
		CharacterTints.SetLocal(_tint, share: false);
		var focused = SpoonWidgets.Focusables(_rows).FindIndex(c => c.HasFocus());
		Build(focused, appeared: false);
	}

	private void Build(int focused, bool appeared)
	{
		foreach (Node child in _rows.GetChildren())
		{
			_rows.RemoveChild(child);
			child.QueueFree();
		}
		var title = new Label { Text = Text("title"), HorizontalAlignment = HorizontalAlignment.Center };
		title.AddThemeFontSizeOverride("font_size", 26);
		title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		_rows.AddChild(title);
		var help = new Label { Text = Text("help"), AutowrapMode = TextServer.AutowrapMode.WordSmart,
			CustomMinimumSize = new Vector2(530, 0) };
		help.AddThemeFontSizeOverride("font_size", 15);
		_rows.AddChild(help);

		var modeLoc = Loc("mode");
		modeLoc.Add("Mode", Text("mode_" + _tint.Mode.ToString().ToLowerInvariant()));
		SpoonWidgets.Stepper(_rows, modeLoc.GetFormattedText(), null,
			delta => Change(_tint with { Mode = (TintMode)(((int)_tint.Mode + Math.Sign(delta) + 3) % 3) }), 1);
		if (_tint.Active)
		{
			var hue = Loc("hue");
			hue.Add("Hue", _tint.Hue);
			var hueRow = SpoonWidgets.Stepper(_rows, hue.GetFormattedText(), null,
				delta => Change(_tint with { Hue = ((_tint.Hue + delta) % 360 + 360) % 360 }), 5, 30);
			var swatch = TintMath.Pure(_tint.Hue / 360f);
			hueRow.AddThemeColorOverride("font_color", new Color(swatch.R, swatch.G, swatch.B));
			var strength = Loc("strength");
			strength.Add("Strength", _tint.Strength);
			SpoonWidgets.Stepper(_rows, strength.GetFormattedText(), null,
				delta => Change(_tint with { Strength = _tint.Strength + delta }), 5, 20);
			if (_tint.Mode == TintMode.Dominant)
			{
				var threshold = Loc("threshold");
				threshold.Add("Threshold", _tint.Threshold);
				SpoonWidgets.Stepper(_rows, threshold.GetFormattedText(), null,
					delta => Change(_tint with { Threshold = _tint.Threshold + delta }), 5, 20);
			}
			SpoonWidgets.Stepper(_rows, Text(_tint.Map ? "map_on" : "map_off"), null,
				_ => Change(_tint with { Map = !_tint.Map }), 1);
		}
		var close = SpoonWidgets.Button(_rows, Text("close"), Close);
		close.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		SpoonWidgets.RestoreFocus(_rows, focused, appeared);
	}

	private void Close()
	{
		CharacterTints.SetLocal(_tint, share: true);
		_overlay.QueueFree();
		if (GodotObject.IsInstanceValid(_returnFocus)) _returnFocus.GrabFocus();
	}
}
