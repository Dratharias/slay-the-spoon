using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Spoon;

namespace SlaySpoon.Act4;

/// <summary>
/// Static StS1 atlas regions; Glory is retained only for the map and unrelated room assets. The Spoon act has its own
/// combat background (images/spoon) and borrows the Ending's campfire.
/// </summary>
[HarmonyPatch]
internal static class EndingBackgroundPatch
{
	internal static TextureRect Image(string name, string folder = "ending")
	{
		var image = new TextureRect
		{
			Name = "Sts1EndingBackground",
			Texture = ResourceLoader.Load<Texture2D>($"res://SlaySpoon/images/{folder}/{name}.jpg"),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		image.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		return image;
	}

	// The combat room's BgContainer is a zero-size Control at the screen centre, and vanilla layers draw around its
	// origin: a full-rect child there has no size and showed nothing. Centre a fixed box on that origin instead, wider
	// than the 1920×1080 view so screen shake keeps the edges hidden (combat_bg.jpg is 1920×1136).
	internal const float CombatHalfWidth = 1100f, CombatHalfHeight = 651f;

	[HarmonyPatch(typeof(NCombatBackground), nameof(NCombatBackground.Create))]
	[HarmonyPrefix]
	private static bool Combat(ref NCombatBackground __result)
	{
		string? folder = Act4Routing.CurrentRun()?.Act switch
		{
			Sts1EndingAct => "ending",
			SpoonAct => "spoon",
			_ => null,
		};
		if (folder == null) return true;
		__result = new NCombatBackground { MouseFilter = Control.MouseFilterEnum.Ignore };
		TextureRect image = Image("combat_bg", folder);
		image.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		image.OffsetLeft = -CombatHalfWidth;
		image.OffsetTop = -CombatHalfHeight;
		image.OffsetRight = CombatHalfWidth;
		image.OffsetBottom = CombatHalfHeight;
		__result.AddChild(image);
		return false;
	}

	internal const float RestLeft = -86f, RestTop = -134f, RestWidth = 2040f, RestHeight = 1200f;

	[HarmonyPatch(typeof(ActModel), nameof(ActModel.CreateRestSiteBackground))]
	[HarmonyPrefix]
	private static bool Rest(ActModel __instance, ref Control __result)
	{
		if (__instance is not (Sts1EndingAct or SpoonAct)) return true;
		// The rest site room's BgContainer is a near-zero-size Control whose origin sits at (26, 74) on the 1920×1080
		// view (centre-anchored, offset −934, −466): a full-rect child there has no size and the room showed black.
		// Give the root a fixed box covering the view from that origin, with a margin for screen shake.
		var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		root.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		root.OffsetLeft = RestLeft;
		root.OffsetTop = RestTop;
		root.OffsetRight = RestLeft + RestWidth;
		root.OffsetBottom = RestTop + RestHeight;
		root.AddChild(Image("campfire_bg"));
		// The room fades this node when resting. The characters and their interactions remain vanilla.
		var lighting = new Control { Name = "RestSiteLighting", MouseFilter = Control.MouseFilterEnum.Ignore };
		root.AddChild(lighting);
		lighting.Owner = root;
		lighting.UniqueNameInOwner = true;
		__result = root;
		return false;
	}
}
