using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SlaySpoon.Act4;

/// <summary>
/// The save writer drops empty id lists (<c>SaveIfNotCollectionEmptyOrNull</c>) and the reader then sees null, but
/// <c>RoomSet.FromSave</c> calls <c>Select</c> on every list. Vanilla acts never have an empty list; the Ending has no
/// events and no normal fights, so every save taken after the team's third key failed validation and the game renamed
/// it <c>.corrupt</c>. Missing lists read as empty here, and before the host sends a loaded save to a client.
/// </summary>
internal static class EndingRoomSetLoadPatch
{
	internal static void FillMissingLists(SerializableRoomSet save)
	{
		save.EventIds ??= new List<ModelId>();
		save.NormalEncounterIds ??= new List<ModelId>();
		save.EliteEncounterIds ??= new List<ModelId>();
	}

	[HarmonyPatch(typeof(RoomSet), nameof(RoomSet.FromSave))]
	private static class OnLoad
	{
		private static void Prefix(SerializableRoomSet save) => FillMissingLists(save);
	}

	[HarmonyPatch(typeof(SerializableRoomSet), nameof(SerializableRoomSet.Serialize))]
	private static class OnSend
	{
		private static void Prefix(SerializableRoomSet __instance) => FillMissingLists(__instance);
	}
}
