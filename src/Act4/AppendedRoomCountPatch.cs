using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Act4;

/// <summary>Appended acts keep their room count in multiplayer (see <see cref="IAppendedAct.RoomCount"/>).</summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GetNumberOfRooms))]
internal static class AppendedRoomCountPatch
{
	[HarmonyPostfix]
	private static void Fixed(ActModel __instance, ref int __result)
	{
		if (__instance is IAppendedAct act) __result = act.RoomCount;
	}
}
