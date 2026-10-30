using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SlaySpoon.Act4;

/// <summary>An Ending monster drawn from one static StS1 setup-pose sprite instead of a vanilla creature scene.</summary>
public interface IEndingMonster
{
	string TexturePath { get; }
	/// <summary>The skeleton root (ground point) in image pixels, from the top-left.</summary>
	Vector2 Origin { get; }
	Vector2 Size { get; }
	/// <summary>Hover and health-bar box relative to the ground point; intents sit above it.</summary>
	Rect2 Hitbox { get; }
}

/// <summary>
/// Builds the creature visuals of an <see cref="IEndingMonster"/> in code. The body is a plain Sprite2D, not a
/// SpineSprite, so the creature has no Spine animator: attacks and hurt play no animation, death uses the
/// hitbox-sized fade.
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals))]
public static class EndingVisuals
{
	private const float IntentGap = 60f;

	public static NCreatureVisuals Create(string texturePath, Vector2 origin, Rect2 hitbox)
	{
		var root = new NCreatureVisuals { Name = "EndingVisuals" };
		// Offset rather than position, so a horizontal flip of the body mirrors around the ground point.
		Add(root, new Sprite2D
		{
			Name = "Visuals",
			Texture = PreloadManager.Cache.GetTexture2D(texturePath),
			Centered = false,
			Offset = -origin,
		});
		Add(root, new Control
		{
			Name = "Bounds",
			Position = hitbox.Position,
			Size = hitbox.Size,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		});
		Vector2 center = hitbox.GetCenter();
		Add(root, new Marker2D { Name = "IntentPos", Position = new Vector2(center.X, hitbox.Position.Y - IntentGap) });
		Add(root, new Marker2D { Name = "CenterPos", Position = center });
		Add(root, new Marker2D { Name = "TalkPos", Position = new Vector2(center.X, hitbox.Position.Y) });
		return root;
	}

	/// <summary>NCreatureVisuals._Ready resolves its parts as %Name, so each one is unique in its owner, the root.</summary>
	private static void Add(Node root, Node child)
	{
		root.AddChild(child);
		child.Owner = root;
		child.UniqueNameInOwner = true;
	}

	private static bool Prefix(MonsterModel __instance, ref NCreatureVisuals __result)
	{
		if (__instance is not IEndingMonster monster) return true;
		__result = Create(monster.TexturePath, monster.Origin, monster.Hitbox);
		return false;
	}
}
