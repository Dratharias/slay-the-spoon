using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace SlaySpoon.Spine;

/// <summary>
/// StS1 Spine skeletons converted to Spine 4.2 by <c>make sts1-spine</c> (<c>godot/SlaySpoon/spine/&lt;key&gt;/</c>),
/// loaded at runtime through the game's own spine-godot classes. The skeleton file is read raw from the PCK and the
/// atlas pages are imported textures, so no Spine importer is needed at export time. Every entry point returns null
/// instead of throwing: callers keep their static sprite then.
/// </summary>
public static class Sts1Spine
{
	public const string Root = "res://SlaySpoon/spine/";

	public const string Heart = "heart", Spear = "spear", Shield = "shield", Watcher = "watcher", WatcherEye = "watcher_eye";

	/// <summary>Loaded skeleton data, shared by every sprite of that key (null: the load failed, don't retry).</summary>
	private static readonly Dictionary<string, GodotObject?> Data = new();

	/// <summary>A SpineSprite showing <paramref name="key"/>'s setup pose, its skeleton root at the node's origin.</summary>
	public static Node2D? CreateSprite(string key, string name)
	{
		try
		{
			if (LoadData(key) is not { } data) return null;
			Node2D? node = ClassDB.Instantiate(MegaSprite.spineClassName).As<Node2D>();
			if (node == null) return null;
			node.Name = name;
			node.Set("skeleton_data_res", data);
			if (new MegaSprite(node).GetSkeleton()?.GetData() == null)
			{
				SpoonLog.Error($"spine: {key} built no skeleton");
				node.Free();
				return null;
			}
			return node;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spine: {key} sprite failed: {e}");
			return null;
		}
	}

	private static GodotObject? LoadData(string key)
	{
		if (Data.TryGetValue(key, out GodotObject? cached)) return cached;
		GodotObject? data = null;
		try
		{
			string dir = Root + key + "/";
			GodotObject atlas = ClassDB.Instantiate("SpineAtlasResource").AsGodotObject();
			var atlasError = (Error)atlas.Call("load_from_atlas_file", dir + "skeleton.atlas").AsInt64();
			GodotObject file = ClassDB.Instantiate("SpineSkeletonFileResource").AsGodotObject();
			var fileError = (Error)file.Call("load_from_file", dir + "skeleton.spine-json").AsInt64();
			if (atlasError != Error.Ok || fileError != Error.Ok)
			{
				SpoonLog.Error($"spine: {key} atlas {atlasError}, skeleton file {fileError}");
			}
			else
			{
				GodotObject res = ClassDB.Instantiate("SpineSkeletonDataResource").AsGodotObject();
				// StS1 mixes the eye's stance changes over 0.2 s (eyeStateData.setDefaultMix).
				if (key == WatcherEye) res.Set("default_mix", 0.2f);
				res.Set("atlas_res", atlas);
				res.Set("skeleton_file_res", file);
				if (res.Call("is_skeleton_data_loaded").AsBool()) data = res;
				else SpoonLog.Error($"spine: {key} skeleton data did not load");
			}
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spine: {key} data failed: {e}");
		}
		Data[key] = data;
		return data;
	}

	/// <summary>
	/// The vanilla creature animator, limited to the animations the skeleton has (the converted StS1 skeletons carry
	/// idle_loop and some of attack and hurt), so a trigger with no animation keeps the current one instead of logging a
	/// warning. "Dead" plays hurt once (StS1 creatures have no death animation; the corpse or the fade follows).
	/// <paramref name="extra"/> adds triggers of the creature's own, each to a one-shot animation.
	/// </summary>
	public static CreatureAnimator Animator(MegaSprite controller, IReadOnlyDictionary<string, string>? extra = null)
	{
		var idle = new AnimState("idle_loop", isLooping: true);
		var animator = new CreatureAnimator(idle, controller);
		animator.AddAnyState(CreatureAnimator.idleTrigger, idle);
		var triggers = new List<(string Trigger, string Animation)>
		{
			(CreatureAnimator.attackTrigger, "attack"),
			(CreatureAnimator.castTrigger, "cast"),
			(CreatureAnimator.powerUpTrigger, "cast"),
			(CreatureAnimator.hitTrigger, "hurt"),
		};
		if (extra != null) triggers.AddRange(extra.Select(p => (p.Key, p.Value)));
		foreach ((string trigger, string animation) in triggers)
		{
			if (!controller.HasAnimation(animation)) continue;
			animator.AddAnyState(trigger, new AnimState(animation) { NextState = idle });
		}
		if (controller.HasAnimation("hurt"))
		{
			animator.AddAnyState(CreatureAnimator.deathTrigger, new AnimState("hurt"));
		}
		return animator;
	}

	/// <summary>World position (in the sprite's own space) of <paramref name="bone"/>, or null if it has none.</summary>
	public static Vector2? BonePosition(Node2D sprite, string bone)
	{
		GodotObject? skeleton = sprite.Call("get_skeleton").AsGodotObject();
		GodotObject? found = skeleton?.Call("find_bone", bone).AsGodotObject();
		if (found == null) return null;
		return new Vector2((float)found.Call("get_world_x").AsDouble(), (float)found.Call("get_world_y").AsDouble());
	}

	/// <summary>Plays <paramref name="animation"/> on track 0 if the sprite's skeleton has it.</summary>
	public static void Play(Node2D sprite, string animation, bool loop)
	{
		var mega = new MegaSprite(sprite);
		if (mega.HasAnimation(animation)) mega.GetAnimationState().SetAnimation(animation, loop);
	}
}
