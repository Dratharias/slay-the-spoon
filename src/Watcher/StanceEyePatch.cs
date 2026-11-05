using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Characters;
using SlaySpoon.Core;
using SlaySpoon.Spine;

namespace SlaySpoon.Watcher;

/// <summary>
/// The Watcher's staff eye shows her stance. StS1 draws it with a separate Spine skeleton placed at the body's eye_anchor
/// bone each frame, playing None, Calm, Wrath or Divinity (0.2 s mix). With her Spine body we do the same: the eye
/// skeleton is a child of the body that follows the bone. With the static fallback body, a static overlay sits on the
/// sprite instead: the eye's white plus the stance's iris, lids left out (open); in Neutral it hides and the closed eye
/// baked into the body shows.
/// </summary>
[HarmonyPatch]
public static class StanceEyePatch
{
	private const string NodeName = "StanceEye";

	/// <summary>Updates the eye of <paramref name="creature"/>'s combat body; does nothing outside combat or for others.</summary>
	public static void Show(Creature creature, Stance stance)
	{
		if (creature.Player?.Character is not Sts1Watcher)
		{
			return;
		}
		if (NCombatRoom.Instance?.GetCreatureNode(creature)?.Body is not { } body || !GodotObject.IsInstanceValid(body))
		{
			return;
		}
		ShowBody(body, stance);
	}

	private static void ShowBody(Node2D body, Stance stance)
	{
		if (body is not Sprite2D)
		{
			ShowSpineEye(body, stance);
			return;
		}
		Node2D? eye = body.GetNodeOrNull<Node2D>(NodeName);
		if (stance == Stance.Neutral)
		{
			if (eye != null)
			{
				eye.Visible = false;
			}
			return;
		}
		eye ??= Create(body);
		eye.GetNode<Sprite2D>("Iris").Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.EyeIris(stance));
		eye.Visible = true;
	}

	// StS2 does not save combat, so there is nothing to restore: the eye is rebuilt from the creature's current powers
	// whenever its node is (re)created. Use the node directly, because the room may not have registered it in its
	// creature lookup yet.
	[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
	[HarmonyPostfix]
	public static void RestoreEye(NCreature __instance)
	{
		if (__instance.Entity.Player?.Character is not Sts1Watcher) return;
		Stance stance = StanceCmd.Current(__instance.Entity);
		ShowBody(__instance.Body, stance);
		StanceFx.Restore(__instance, stance);
	}

	private const string AnchorBone = "eye_anchor";

	/// <summary>StS1's eye animation for each stance (Neutral: "None", lids closed).</summary>
	private static string EyeAnimation(Stance stance) => stance == Stance.Neutral ? "None" : stance.ToString();

	private static void ShowSpineEye(Node2D body, Stance stance)
	{
		Node2D? eye = body.GetNodeOrNull<Node2D>(NodeName);
		if (eye == null)
		{
			eye = Sts1Spine.CreateSprite(Sts1Spine.WatcherEye, NodeName);
			if (eye == null)
			{
				// No eye skeleton: the static overlay, open eye in a stance and nothing in Neutral.
				if (stance != Stance.Neutral) StaticEye(Create(body), stance);
				return;
			}
			body.AddChild(eye);
			Follow(body, eye);
		}
		if (eye.GetClass() != MegaSprite.spineClassName)
		{
			eye.Visible = stance != Stance.Neutral;
			if (eye.Visible) StaticEye(eye, stance);
			return;
		}
		string animation = EyeAnimation(stance);
		if (eye.GetMeta(AnimationMeta, "").AsString() != animation)
		{
			eye.SetMeta(AnimationMeta, animation);
			Sts1Spine.Play(eye, animation, loop: true);
		}
		eye.Visible = true;
	}

	private const string AnimationMeta = "spoon_eye_animation";

	private static void StaticEye(Node2D eye, Stance stance) =>
		eye.GetNode<Sprite2D>("Iris").Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.EyeIris(stance));

	/// <summary>Keeps the eye on the anchor bone after every body update, as StS1 does each frame (no rotation).</summary>
	private static void Follow(Node2D body, Node2D eye)
	{
		void Place()
		{
			if (!GodotObject.IsInstanceValid(eye) || !GodotObject.IsInstanceValid(body)) return;
			if (Sts1Spine.BonePosition(body, AnchorBone) is { } at) eye.Position = at;
		}
		Place();
		body.Connect("world_transforms_changed", Callable.From((GodotObject _) => Place()));
	}

	private static Node2D Create(Node2D body)
	{
		// The body's ground point is its origin (it draws at Offset = -origin), so the anchor is relative to it.
		var eye = new Node2D
		{
			Name = NodeName,
			Position = new Vector2(WatcherAssets.EyeAnchorX, WatcherAssets.EyeAnchorY),
			RotationDegrees = WatcherAssets.EyeRotationDegrees,
		};
		eye.AddChild(new Sprite2D { Name = "Base", Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.EyeBase) });
		eye.AddChild(new Sprite2D { Name = "Iris" });
		body.AddChild(eye);
		return eye;
	}

	/// <summary>The corpse has no staff eye: hide the overlay when she dies (a revive comes back in Neutral).</summary>
	[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
	[HarmonyPostfix]
	public static void HideOnDeath(NCreature __instance)
	{
		StanceFx.Died(__instance);
		// A Spine body is hidden whole (eye included) for the corpse; a static body keeps showing, so hide its overlay.
		if (__instance.Body is Sprite2D body && body.GetNodeOrNull<Node2D>(NodeName) is { } eye)
		{
			eye.Visible = false;
		}
	}
}
