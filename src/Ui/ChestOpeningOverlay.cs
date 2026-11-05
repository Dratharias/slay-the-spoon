using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;

namespace SlaySpoon.Ui;

/// <summary>
/// The act's treasure chest (its Spine skeleton, normal skin and open sound, as <c>NTreasureRoom</c> sets them up)
/// opening over a dimmed screen, for the local player only. Never throws: a failure only skips the animation.
/// </summary>
public static class ChestOpeningOverlay
{
	private const double OpenSeconds = 1.6;
	private const double FadeSeconds = 0.3;

	public static async Task Play(ActModel act)
	{
		CanvasLayer? layer = null;
		try
		{
			if (NGame.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return;
			layer = new CanvasLayer { Name = "SpoonChestOpening", Layer = 50 };
			var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop };
			dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			layer.AddChild(dim);
			var chest = ClassDB.Instantiate(MegaSprite.spineClassName).As<Node2D>()
				?? throw new InvalidOperationException("SpineSprite is not available");
			Vector2 size = game.GetViewport().GetVisibleRect().Size;
			chest.Position = new Vector2(size.X / 2f, size.Y * 0.62f);
			layer.AddChild(chest);
			game.AddChild(layer);

			var sprite = new MegaSprite(chest);
			sprite.SetSkeletonDataRes(act.ChestSpineResource);
			if (sprite.GetSkeleton() is { } skeleton)
			{
				skeleton.SetSkin(skeleton.GetData().FindSkin(act.ChestSpineSkinNameNormal));
				skeleton.SetSlotsToSetupPose();
				MegaAnimationState state = sprite.GetAnimationState();
				state.Apply(skeleton);
				state.SetAnimation("animation", loop: false);
				state.AddAnimation("shine_fade", 0f, loop: false);
				state.SetTimeScale(1f);
			}
			SfxCmd.Play(act.ChestOpenSfx);
			await Wait(layer, OpenSeconds);
			Tween fade = layer.CreateTween();
			fade.TweenProperty(dim, "color:a", 0f, FadeSeconds);
			fade.Parallel().TweenProperty(chest, "modulate:a", 0f, FadeSeconds);
			await Wait(layer, FadeSeconds);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"starting chest: opening animation failed: {e}");
		}
		finally
		{
			if (layer != null && GodotObject.IsInstanceValid(layer)) layer.QueueFree();
		}
	}

	private static async Task Wait(Node node, double seconds) =>
		await node.ToSignal(node.GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
}
