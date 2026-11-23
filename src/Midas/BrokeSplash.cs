using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;

namespace SlaySpoon.Midas;

/// <summary>
/// "Broke": a big word over a greyed screen when the melt ruins the local player (owner, 2026-11-23, a nod to action
/// games' death caption). View only: shown on the ruined player's screen, never blocks input, frees itself.
/// </summary>
public static class BrokeSplash
{
	public static void Show()
	{
		try
		{
			if (NGame.Instance is not { } game) return;
			var layer = new CanvasLayer { Name = "SpoonBroke", Layer = 60 };
			var dim = new ColorRect { Color = new Color(0.08f, 0.08f, 0.08f, 0.55f), MouseFilter = Control.MouseFilterEnum.Ignore };
			dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			var label = new Label
			{
				Text = new LocString("rest_site_ui", "SPOON_MIDAS.broke").GetFormattedText(),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};
			label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			label.AddThemeFontOverride("font", GD.Load<Font>("res://themes/kreon_bold_shared.tres"));
			label.AddThemeFontSizeOverride("font_size", 160);
			label.AddThemeColorOverride("font_color", new Color(0.95f, 0.75f, 0.2f));
			label.AddThemeColorOverride("font_outline_color", new Color(0.05f, 0.03f, 0f));
			label.AddThemeConstantOverride("outline_size", 24);
			var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1f, 1f, 1f, 0f) };
			root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			root.AddChild(dim);
			root.AddChild(label);
			layer.AddChild(root);
			game.AddChild(layer);
			label.PivotOffset = game.GetViewport().GetVisibleRect().Size / 2f;
			label.Scale = new Vector2(1.25f, 1.25f);
			Tween tween = root.CreateTween();
			tween.TweenProperty(root, "modulate:a", 1f, 0.35);
			tween.Parallel().TweenProperty(label, "scale", Vector2.One, 0.6).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tween.TweenInterval(1.8);
			tween.TweenProperty(root, "modulate:a", 0f, 0.6);
			tween.TweenCallback(Callable.From(layer.QueueFree));
		}
		catch (Exception e)
		{
			SpoonLog.Info($"King Midas: broke splash failed: {e.Message}");
		}
	}
}
