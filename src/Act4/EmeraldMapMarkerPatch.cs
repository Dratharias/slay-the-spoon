using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// Marks the burning elite like StS1 does: green flames licking the node (StS1's map flame frames) and the
/// emerald gem beside it, until the team holds the key.
/// </summary>
[HarmonyPatch(typeof(NNormalMapPoint), nameof(NNormalMapPoint._Ready))]
internal static class EmeraldMapMarkerPatch
{
	private static readonly AccessTools.FieldRef<NMapPoint, IRunState> Run =
		AccessTools.FieldRefAccess<NMapPoint, IRunState>("_runState");

	private const int FlameCount = 6;
	private const float GemSize = 44f;
	private static readonly Color FlameColor = new(0.15f, 0.85f, 0.35f);

	private static void Postfix(NNormalMapPoint __instance)
	{
		var run = Run(__instance);
		if (SpoonAct4.Of(run) is not { } act4) return;
		var coord = __instance.Point.coord;
		if (act4.Keys.EmeraldNode(run.CurrentActIndex) != (coord.col, coord.row)) return;

		// Behind the node's own icon.
		var flames = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		__instance.AddChild(flames);
		__instance.MoveChild(flames, 0);
		// Godot's own loader, not PreloadManager.Cache: the game disposes the textures its cache loaded outside
		// a preload at the next room boundary, and the flame tweens keep reusing these frames after it.
		var frames = new Texture2D[6];
		for (int i = 0; i < frames.Length; i++)
		{
			frames[i] = ResourceLoader.Load<Texture2D>($"res://SlaySpoon/images/map/flame_{i}.png");
		}
		for (int i = 0; i < FlameCount; i++)
		{
			AddFlame(__instance, flames, frames, i / (float)FlameCount);
		}
		var gem = new TextureRect
		{
			Texture = new AtlasTexture
			{
				Atlas = ResourceLoader.Load<Texture2D>(KeySprites.Path(Act4Key.Emerald, large: true)),
				Region = new Rect2(70, 70, 116, 116),
			},
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Size = new Vector2(GemSize, GemSize),
		};
		__instance.AddChild(gem);
		void Place() => gem.Position = new Vector2(__instance.Size.X - GemSize * 0.35f, -GemSize * 0.55f);
		__instance.Resized += Place;
		Place();

		void Refresh() => flames.Visible = gem.Visible = !act4.Keys.Has(Act4Key.Emerald);
		Act4Keys.Changed += Refresh;
		__instance.TreeExiting += () => Act4Keys.Changed -= Refresh;
		Refresh();
	}

	/// <summary>One flame that plays StS1's six frames, rising and fading, then respawns elsewhere on the node.</summary>
	private static void AddFlame(Control node, Control parent, Texture2D[] frames, float phase)
	{
		var flame = new TextureRect
		{
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Modulate = FlameColor,
		};
		parent.AddChild(flame);
		var start = Vector2.Zero;
		var size = 0f;
		void Respawn()
		{
			var width = Mathf.Max(node.Size.X, 48f);
			size = (float)GD.RandRange(0.7, 1.0) * width;
			start = node.Size / 2f + new Vector2((float)GD.RandRange(-0.4, 0.4) * width, (float)GD.RandRange(-0.1, 0.4) * width);
			flame.Size = new Vector2(size, size);
			flame.FlipH = GD.Randf() < 0.5f;
		}
		void Step(float t)
		{
			if (!GodotObject.IsInstanceValid(flame)) return;
			flame.Texture = frames[Mathf.Min((int)(t * frames.Length), frames.Length - 1)];
			flame.Position = start - new Vector2(size / 2f, size * 0.8f + t * size * 0.25f);
			flame.SelfModulate = new Color(1, 1, 1, 0.9f * Mathf.Sin(Mathf.Pi * t));
		}
		Respawn();
		Step(phase);
		var tween = flame.CreateTween().SetLoops();
		tween.TweenCallback(Callable.From(Respawn));
		tween.TweenMethod(Callable.From<float>(Step), 0f, 1f, GD.RandRange(0.6, 0.8));
		// Stagger the flames so they do not pulse together.
		tween.CustomStep(phase * 0.7f);
	}
}
