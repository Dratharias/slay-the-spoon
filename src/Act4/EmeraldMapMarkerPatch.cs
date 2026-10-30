using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

[HarmonyPatch(typeof(NNormalMapPoint), nameof(NNormalMapPoint._Ready))]
internal static class EmeraldMapMarkerPatch
{
	private static readonly AccessTools.FieldRef<NMapPoint, IRunState> Run =
		AccessTools.FieldRefAccess<NMapPoint, IRunState>("_runState");

	private static void Postfix(NNormalMapPoint __instance)
	{
		var run = Run(__instance);
		if (SpoonAct4.Of(run) is not { } act4) return;
		var coord = __instance.Point.coord;
		if (act4.Keys.EmeraldNode(run.CurrentActIndex) != (coord.col, coord.row)) return;
		var marker = new TextureRect
		{
			Texture = PreloadManager.Cache.GetTexture2D(KeySprites.Path(Act4Key.Emerald, large: false)),
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Position = new Vector2(16, -24), Size = new Vector2(24, 32),
		};
		__instance.AddChild(marker);
		void Refresh() => marker.Visible = !act4.Keys.Has(Act4Key.Emerald);
		Act4Keys.Changed += Refresh;
		__instance.TreeExiting += () => Act4Keys.Changed -= Refresh;
		Refresh();
	}
}
