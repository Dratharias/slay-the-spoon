using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace SlaySpoon.Act4;

/// <summary>
/// The vanilla "defeat 15 elites" character epoch counts every elite encounter in <c>ModelDb</c>, which includes the
/// Spire Shield and Spear. Keep that fight out of the count, as the boss epoch already leaves the Heart out (its list
/// is <c>ModelDb.Acts</c>, which never holds the Ending act). The set is cached by the game; removing is idempotent.
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "GetEliteEncounters")]
internal static class EndingEliteEpochPatch
{
	private static void Postfix(HashSet<ModelId> __result) => __result.Remove(ModelDb.GetId<Sts1ShieldAndSpearElite>());
}
