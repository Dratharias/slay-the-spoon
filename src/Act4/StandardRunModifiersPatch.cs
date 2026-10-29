using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Saves;

namespace SlaySpoon.Act4;

/// <summary>
/// Standard runs carry no modifiers, so the Act 4 flag has to be slipped in on the way from the lobby to the run:
/// the host's lobby adds it (solo and co-op both go through <c>StartRunLobby.BeginRunForAllPlayers</c>, and clients
/// receive it in <c>LobbyBeginRunMessage</c>), the Standard character select would reject a non-empty list, so it
/// parks the list in <see cref="Pending"/> and hands vanilla an empty one, and <c>NGame</c> gets it back at run creation.
/// </summary>
public static class StandardRunModifiers
{
	/// <summary>The modifiers of the Standard run being started, parked between character select and NGame.</summary>
	internal static List<ModifierModel> Pending = new();

	internal static bool Wanted => Act4Access.InStandardRun(SpoonConfigFile.Act4Unlocked(Act4UnlockPatch.CurrentProfileId()),
		SpoonConfigFile.Act4Preview);

	internal static IReadOnlyList<ModifierModel> TakePending(IReadOnlyList<ModifierModel> modifiers)
	{
		List<ModifierModel> pending = Pending;
		Pending = new List<ModifierModel>();
		return pending.Count > 0 ? pending : modifiers;
	}
}

/// <summary>Host (and solo): adds the flag to a Standard run's modifier list when Act 4 is available.</summary>
[HarmonyPatch(typeof(StartRunLobby), "BeginRunForAllPlayers")]
public static class StandardRunLobbyPatch
{
	public static void Prefix(StartRunLobby __instance, ref List<ModifierModel> modifiers)
	{
		try
		{
			if (__instance.GameMode != GameMode.Standard || modifiers.Any(m => m is SpoonAct4) || !StandardRunModifiers.Wanted)
			{
				return;
			}
			modifiers = new List<ModifierModel>(modifiers) { ModelDb.Modifier<SpoonAct4>().ToMutable() };
			SpoonLog.Info("act4: flag added to the Standard run");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"act4: could not add the run flag: {e.Message}");
		}
	}
}

/// <summary>Standard character select refuses modifiers; park them and pass an empty list.</summary>
[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.BeginRun))]
public static class StandardCharacterSelectPatch
{
	public static void Prefix(ref IReadOnlyList<ModifierModel> modifiers)
	{
		StandardRunModifiers.Pending = new List<ModifierModel>(modifiers);
		modifiers = Array.Empty<ModifierModel>();
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.StartNewSingleplayerRun))]
public static class StandardSoloRunPatch
{
	public static void Prefix(GameMode gameMode, ref IReadOnlyList<ModifierModel> modifiers)
	{
		if (gameMode == GameMode.Standard)
		{
			modifiers = StandardRunModifiers.TakePending(modifiers);
		}
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.StartNewMultiplayerRun))]
public static class StandardCoopRunPatch
{
	public static void Prefix(StartRunLobby lobby, ref IReadOnlyList<ModifierModel> modifiers)
	{
		if (lobby.GameMode == GameMode.Standard)
		{
			modifiers = StandardRunModifiers.TakePending(modifiers);
		}
	}
}
