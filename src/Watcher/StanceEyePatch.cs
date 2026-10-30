using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>
/// The Watcher's staff eye shows her stance. StS1 draws it with a separate Spine skeleton over the body; we have no Spine
/// data, so a static overlay sits on her combat body sprite: the eye's white plus the stance's iris, with the lids left
/// out (open). In Neutral the overlay hides and the closed eye baked into the body shows.
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
		if (NCombatRoom.Instance?.GetCreatureNode(creature)?.Body is not Sprite2D body || !GodotObject.IsInstanceValid(body))
		{
			return;
		}
		ShowBody(body, stance);
	}

	private static void ShowBody(Sprite2D body, Stance stance)
	{
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

	// Saved powers are restored before the creature node is ready. Use the node directly here,
	// because the room may not have registered it in its creature lookup yet.
	[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
	[HarmonyPostfix]
	public static void RestoreEye(NCreature __instance)
	{
		if (__instance.Entity.Player?.Character is Sts1Watcher && __instance.Body is Sprite2D body)
			ShowBody(body, StanceCmd.Current(__instance.Entity));
	}

	private static Node2D Create(Sprite2D body)
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
		if (__instance.Body is Sprite2D body && body.GetNodeOrNull<Node2D>(NodeName) is { } eye)
		{
			eye.Visible = false;
		}
	}
}
