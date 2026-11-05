using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace SlaySpoon.Act4;

/// <summary>
/// Vanilla <c>ActModel.GenerateRooms</c> is not virtual and assumes non-empty weak, normal and ancient lists. For
/// the Ending act it is replaced by <see cref="Sts1EndingAct.FillRooms"/>, which sets the room set directly.
/// <see cref="Sts1EndingAct.CreateForRun"/> already fills the rooms; this guard covers any other caller. It never
/// draws from the caller's Rng (a vanilla one, such as the run's UpFront), only from the act's named Rng on the run
/// seed, the one <c>CreateForRun</c> uses, so both paths give the same rooms and the run's seed stays vanilla.
/// The caller's seed is the fallback outside a run.
/// </summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
internal static class EndingRoomsPatch
{
	private static bool Prefix(ActModel __instance, Rng rng)
	{
		if (__instance is not Sts1EndingAct act) return true;
		uint seed = Act4Routing.CurrentRun()?.Rng.Seed ?? rng.Seed;
		act.FillRooms(new Rng(seed, Sts1EndingAct.RoomsRngName));
		return false;
	}
}
