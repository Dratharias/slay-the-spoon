using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace SlaySpoon.Ui;

/// <summary>Native popup scene, fonts, confirmation buttons and settings sliders, hosted in the overlay stack.</summary>
public sealed partial class NativeTradePopup : Control, IOverlayScreen
{
	public VBoxContainer Rows { get; } = new();
	public VBoxContainer Footer { get; } = new();
	public Action? Cancel { get; set; }
	public event Action? Closed;
	private VBoxContainer _layout = null!;
	private bool _closed;
	public bool IsClosed => _closed;
	public NetScreenType ScreenType => NetScreenType.CardSelection;
	public bool UseSharedBackstop => true;
	public Control? DefaultFocusedControl => SpoonWidgets.Focusables(this).FirstOrDefault();

	public static NativeTradePopup? Open(string title)
	{
		if (NOverlayStack.Instance is not { } stack || NGenericPopup.Create() is not { } native) return null;
		var screen = new NativeTradePopup();
		screen.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		screen.AddChild(native);
		stack.Push(screen);
		var frame = native.GetNode<NVerticalPopup>("VerticalPopup");
		frame.SetText(title, "");
		frame.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		frame.AnchorLeft = 0.12f;
		frame.AnchorRight = 0.88f;
		frame.AnchorTop = 0.08f;
		frame.AnchorBottom = 0.92f;
		frame.Set("stretch_mode", (int)TextureRect.StretchModeEnum.Scale);
		// Keep the game's artwork and typography; controls below have their own local callbacks.
		frame.YesButton.Disable();
		frame.YesButton.Hide();
		frame.NoButton.Disable();
		frame.NoButton.Hide();
		frame.GetNode<Control>("Description").Hide();
		var layout = new VBoxContainer();
		screen._layout = layout;
		frame.AddChild(layout);
		layout.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		layout.OffsetLeft = 56;
		layout.OffsetRight = -56;
		layout.OffsetTop = 135;
		layout.OffsetBottom = -48;
		layout.AddThemeConstantOverride("separation", 20);
		var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			SizeFlagsVertical = SizeFlags.ExpandFill, FollowFocus = true };
		layout.AddChild(scroll);
		layout.AddChild(screen.Footer);
		screen.Footer.AddThemeConstantOverride("separation", 12);
		scroll.AddChild(screen.Rows);
		screen.Rows.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		screen.Rows.AddThemeConstantOverride("separation", 20);
		return screen;
	}

	public void ReservePreviewColumn(float width) => _layout.OffsetLeft += width;

	public static Label Label(Control parent, string text)
	{
		var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsHorizontal = SizeFlags.ExpandFill };
		label.AddThemeFontOverride("font", GD.Load<Font>("res://themes/kreon_regular_glyph_space_one.tres"));
		label.AddThemeFontSizeOverride("font_size", 22);
		label.AddThemeColorOverride("font_color", new Color(1, 0.964706f, 0.886275f));
		parent.AddChild(label);
		return label;
	}

	public static NPopupYesNoButton Button(Control parent, string text, Action action, bool disabled = false, bool positive = true)
	{
		string scene = positive ? "abandon_run_yes_button" : "abandon_run_no_button";
		var button = PreloadManager.Cache.GetScene($"res://scenes/ui/{scene}.tscn").Instantiate<NPopupYesNoButton>();
		button.CustomMinimumSize = new Vector2(180, 72);
		button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		parent.AddChild(button);
		button.SetText(text);
		button.IsYes = positive;
		if (!positive && parent is HBoxContainer) parent.MoveChild(button, 0);
		button.DisconnectHotkeys();
		Callable.From(() => { if (GodotObject.IsInstanceValid(button)) button.DisconnectHotkeys(); }).CallDeferred();
		button.FocusMode = FocusModeEnum.All;
		button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => action()));
		if (disabled) button.Disable();
		return button;
	}

	public static CheckBox Checkbox(Control parent, string text, bool selected, Action<bool> change)
	{
		var check = new CheckBox { Text = text, ButtonPressed = selected,
			CustomMinimumSize = new Vector2(0, 52), FocusMode = FocusModeEnum.All };
		check.AddThemeFontOverride("font", GD.Load<Font>("res://themes/kreon_regular_glyph_space_one.tres"));
		check.AddThemeFontSizeOverride("font_size", 24);
		check.AddThemeColorOverride("font_color", new Color(1, 0.964706f, 0.886275f));
		check.AddThemeIconOverride("checked", PreloadManager.Cache.GetTexture2D("res://images/atlases/ui_atlas.sprites/checkbox_ticked.tres"));
		check.AddThemeIconOverride("unchecked", PreloadManager.Cache.GetTexture2D("res://images/atlases/ui_atlas.sprites/checkbox_unticked.tres"));
		check.AddThemeConstantOverride("h_separation", 18);
		check.Toggled += value => change(value);
		check.GuiInput += input =>
		{
			if (!input.IsActionPressed(MegaInput.select)) return;
			check.AcceptEvent();
			check.ButtonPressed = !check.ButtonPressed;
		};
		parent.AddChild(check);
		return check;
	}

	public static HBoxContainer Actions(Control parent)
	{
		var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		row.AddThemeConstantOverride("separation", 48);
		parent.AddChild(row);
		return row;
	}

	/// <summary>Use the native settings slider scene without a volume-setting script.</summary>
	public static void Slider(Control parent, string title, int value, int maximum, Action<int> change)
	{
		var line = new HBoxContainer();
		line.AddThemeConstantOverride("separation", 120);
		parent.AddChild(line);
		Label(line, title);
		var row = PreloadManager.Cache.GetScene("res://scenes/screens/settings_slider.tscn").Instantiate<Control>();
		line.AddChild(row);
		row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		row.CustomMinimumSize = new Vector2(324, 64);
		var slider = row.GetNode<NSlider>("Slider");
		var number = row.GetNode<MegaLabel>("SliderValue");
		slider.MinValue = 0;
		slider.MaxValue = Math.Max(1, maximum); // NSlider divides by MaxValue while drawing its handle.
		slider.Step = 1;
		slider.Value = value;
		number.SetTextAutoSize(value.ToString());
		if (maximum <= 0) { slider.MouseFilter = MouseFilterEnum.Ignore; row.FocusMode = FocusModeEnum.None; }
		slider.ValueChanged += next => { number.SetTextAutoSize(((int)next).ToString()); change((int)next); };
		row.GuiInput += input =>
		{
			if (maximum <= 0) return;
			int direction = input.IsActionPressed(MegaInput.left, allowEcho: true) ? -1
				: input.IsActionPressed(MegaInput.right, allowEcho: true) ? 1 : 0;
			if (direction == 0) return;
			row.AcceptEvent();
			slider.Value += direction * (input.IsEcho() ? 10 : 1);
		};
		var reticle = row.GetNode<NSelectionReticle>("SelectionReticle");
		row.FocusEntered += reticle.OnSelect;
		row.FocusExited += reticle.OnDeselect;
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if (!_closed && NOverlayStack.Instance?.Peek() == this && input.IsActionPressed(MegaInput.cancel))
		{
			GetViewport().SetInputAsHandled();
			Cancel?.Invoke();
		}
	}

	public void Close()
	{
		if (_closed) return;
		NOverlayStack.Instance?.Remove(this);
	}
	public void AfterOverlayOpened() { }
	public void AfterOverlayShown() => Show();
	public void AfterOverlayHidden() => Hide();
	public void AfterOverlayClosed()
	{
		if (_closed) return;
		_closed = true;
		Closed?.Invoke();
		QueueFree();
	}
}
