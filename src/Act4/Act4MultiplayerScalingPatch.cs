using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Singleton;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>Act 4 uses act 3's elite/boss HP and Block factors (the vanilla method rejects index 3).</summary>
[HarmonyPatch(typeof(MultiplayerScalingModel), nameof(MultiplayerScalingModel.GetMultiplayerScaling))]
internal static class Act4MultiplayerScalingPatch
{
	private static void Prefix(ref int actIndex)
	{
		actIndex = VanillaActIndex.For(actIndex);
	}
}
