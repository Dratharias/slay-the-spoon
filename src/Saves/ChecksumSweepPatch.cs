using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace SlaySpoon.Saves;

/// <summary>
/// Puts every lost detour back right before the game hashes its state for the co-op checksum, so both peers hash with
/// the same patches (some checksums, like the turn start one, are not taken inside a game action).
/// </summary>
[HarmonyPatch(typeof(ChecksumTracker), nameof(ChecksumTracker.GenerateChecksum), typeof(string), typeof(GameAction))]
internal static class ChecksumSweepPatch
{
	public static void Prefix() => SerializationPatchGuard.SweepNow();
}
