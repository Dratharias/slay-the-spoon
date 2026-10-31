using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace SlaySpoon.Act4;

/// <summary>Static StS1 atlas regions; Glory is retained only for the map and unrelated room assets.</summary>
[HarmonyPatch]
internal static class EndingBackgroundPatch
{
	internal static TextureRect Image(string name)
	{
		var image = new TextureRect
		{
			Name = "Sts1EndingBackground",
			Texture = ResourceLoader.Load<Texture2D>($"res://SlaySpoon/images/ending/{name}.jpg"),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		image.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		return image;
	}

	[HarmonyPatch(typeof(NCombatBackground), nameof(NCombatBackground.Create))]
	[HarmonyPrefix]
	private static bool Combat(ref NCombatBackground __result)
	{
		if (Act4Routing.CurrentRun()?.Act is not Sts1EndingAct) return true;
		__result = new NCombatBackground { MouseFilter = Control.MouseFilterEnum.Ignore };
		__result.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		__result.AddChild(Image("combat_bg"));
		return false;
	}

	[HarmonyPatch(typeof(ActModel), nameof(ActModel.CreateRestSiteBackground))]
	[HarmonyPrefix]
	private static bool Rest(ActModel __instance, ref Control __result)
	{
		if (__instance is not Sts1EndingAct) return true;
		var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
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
