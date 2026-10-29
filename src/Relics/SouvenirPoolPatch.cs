using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace SlaySpoon.Relics;

/// <summary>
/// Puts the Spire Souvenir in the Ironclad relic pool. Each player's relic grab bag draws from their own character's
/// pool, so only Ironclad players can find it. The pool caches this list on first read, during ModelDb.Init, which
/// runs after mods are loaded.
/// </summary>
[HarmonyPatch(typeof(IroncladRelicPool), "GenerateAllRelics")]
public static class SouvenirPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonSpireSouvenir>()).ToList();
	}
}
