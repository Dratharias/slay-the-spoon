using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;

namespace SlaySpoon.Act4;

/// <summary>
/// ActModel derives the rest site background, the three map backgrounds, the combat background scene and layers
/// (<c>GenerateBackgroundAssets</c>, <c>GetFullLayerPath</c>, <c>GetAllBackgroundLayerPaths</c>) from the
/// protected, non-virtual <c>FilePathIdentifier</c> (the lowercased id). The appended acts (the Ending, the Spoon)
/// have no such assets of their own, so their identifier is Glory's.
/// </summary>
/// <remarks>
/// sts2.dll is plain IL (not ReadyToRun) and Harmony 2.4 (MonoMod.Core) disables inlining of a patched method,
/// so callers JIT-compiled after PatchAll call the patched getter. If an act 4 rest site ever shows a missing
/// background, the fallback is to patch each public consumer listed above instead.
/// </remarks>
[HarmonyPatch(typeof(ActModel), "FilePathIdentifier", MethodType.Getter)]
internal static class EndingActAssetsPatch
{
	private static readonly Func<ActModel, string> Identifier =
		AccessTools.MethodDelegate<Func<ActModel, string>>(
			AccessTools.PropertyGetter(typeof(ActModel), "FilePathIdentifier"));

	private static void Postfix(ActModel __instance, ref string __result)
	{
		if (__instance is IAppendedAct)
		{
			// Through Glory's canonical model, so the patched getter runs its vanilla path for Glory.
			__result = Identifier(ModelDb.Act<Glory>());
		}
	}
}
