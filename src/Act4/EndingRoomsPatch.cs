using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace SlaySpoon.Act4;

/// <summary>
/// Vanilla <c>ActModel.GenerateRooms</c> is not virtual and assumes non-empty weak, normal and ancient lists. For
/// the Ending act it is replaced by <see cref="Sts1EndingAct.FillRooms"/>, which sets the room set directly.
/// <see cref="Sts1EndingAct.CreateForRun"/> already fills the rooms; this guard covers any other caller. It never
/// draws from the caller's Rng (a vanilla one, such as the run's UpFront), only from a named Rng derived from its
/// seed, so the run's seed stays vanilla.
/// </summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
internal static class EndingRoomsPatch
{
	private static bool Prefix(ActModel __instance, Rng rng)
	{
		if (__instance is not Sts1EndingAct act) return true;
		act.FillRooms(new Rng(rng.Seed, Sts1EndingAct.RoomsRngName));
		return false;
	}
}
