using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.sts2.Core.Nodes.TopBar;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.TopBar;

/// <summary>
/// The mod ships no modifier icons, so the top bar would show the game's "missing" placeholder.
/// Show the percent instead: red when monsters are stronger than vanilla, blue when weaker.
/// The hover tip still names the modifier.
/// </summary>
[HarmonyPatch(typeof(NTopBarModifier), nameof(NTopBarModifier._Ready))]
public static class TopBarPercentPatch
{
	private static readonly AccessTools.FieldRef<NTopBarModifier, MegaCrit.Sts2.Core.Models.ModifierModel> Modifier =
		AccessTools.FieldRefAccess<NTopBarModifier, MegaCrit.Sts2.Core.Models.ModifierModel>("_modifier");
	private static readonly AccessTools.FieldRef<NTopBarModifier, TextureRect> Icon =
		AccessTools.FieldRefAccess<NTopBarModifier, TextureRect>("_icon");

	public static void Postfix(NTopBarModifier __instance)
	{
		if (Modifier(__instance) is not SpoonPercentModifier modifier)
		{
			return;
		}
		TextureRect icon = Icon(__instance);
		icon.Texture = null;

		Label label = new Label
		{
			Name = "SpoonPercent",
			Text = modifier.Percent + "%",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		label.AddThemeFontSizeOverride("font_size", 22);
		label.AddThemeColorOverride("font_color", ScalingMath.Tone(modifier.Percent) switch
		{
			> 0 => StsColors.red,
			< 0 => StsColors.blue,
			_ => StsColors.cream,
		});
		label.AddThemeColorOverride("font_outline_color", Colors.Black);
		label.AddThemeConstantOverride("outline_size", 8);
		icon.AddChild(label);
		label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
	}
}
