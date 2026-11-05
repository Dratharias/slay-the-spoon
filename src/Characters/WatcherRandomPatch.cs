using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Characters;

/// <summary>
/// The Random button resolves through <c>rng.NextItem(ModelDb.AllCharacters)</c> in BeginRunLocally. In a solo
/// Standard lobby, where her button is offered, that draw also includes the Watcher; ModelDb.AllCharacters itself
/// stays vanilla, so co-op, Custom Run, Daily and every other reader are unchanged.
/// </summary>
[HarmonyPatch(typeof(StartRunLobby), "BeginRunLocally")]
public static class WatcherRandomPatch
{
	private static readonly MethodInfo AllCharacters =
		AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllCharacters));

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(AllCharacters))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WatcherRandomPatch), nameof(RandomPool)));
				replaced++;
				continue;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			SpoonLog.Error($"watcher: expected one ModelDb.AllCharacters read in BeginRunLocally, found {replaced}; Random stays vanilla");
		}
	}

	public static IEnumerable<CharacterModel> RandomPool(StartRunLobby lobby)
	{
		IEnumerable<CharacterModel> vanilla = ModelDb.AllCharacters;
		return WatcherRunRule.RandomIncludesWatcher(lobby.NetService.Type == NetGameType.Singleplayer, lobby.GameMode == GameMode.Standard)
			? vanilla.Append(ModelDb.Character<Sts1Watcher>()).ToArray()
			: vanilla;
	}
}
