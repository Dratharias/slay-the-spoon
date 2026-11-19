using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Characters;

/// <summary>
/// The Random button resolves through <c>rng.NextItem(ModelDb.AllCharacters)</c> in BeginRunLocally. In a solo
/// Standard or Custom lobby (solo or co-op), where her button is offered, that draw also includes the Watcher; every
/// client of a co-op lobby makes the same call on the same seed. ModelDb.AllCharacters itself stays vanilla, so Daily
/// and every other reader are unchanged.
/// BaseLib (a dependency) transpiles the same read into its <c>GetRandomEligibleCharacters()</c>. The original call is
/// kept, whichever of the two it is, and its result is extended afterwards, so the patch order does not matter.
/// </summary>
[HarmonyPatch(typeof(StartRunLobby), "BeginRunLocally")]
public static class WatcherRandomPatch
{
	private static readonly MethodInfo AllCharacters =
		AccessTools.PropertyGetter(typeof(ModelDb), nameof(ModelDb.AllCharacters));

	/// <summary>BaseLib's replacement for the AllCharacters read (BaseLib.Abstracts.VanillaRandomCharacterEligibilityPatch).</summary>
	private const string BaseLibReplacement = "GetRandomEligibleCharacters";

	internal static bool IsCharacterListRead(CodeInstruction instruction) =>
		instruction.Calls(AllCharacters)
		|| (instruction.operand is MethodInfo { Name: BaseLibReplacement } m
			&& typeof(IEnumerable<CharacterModel>).IsAssignableFrom(m.ReturnType));

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int extended = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
			if (IsCharacterListRead(instruction))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WatcherRandomPatch), nameof(RandomPool)));
				extended++;
			}
		}
		if (extended != 1)
		{
			SpoonLog.Error($"watcher: expected one character list read in BeginRunLocally, found {extended}; Random stays vanilla");
		}
	}

	public static IEnumerable<CharacterModel> RandomPool(IEnumerable<CharacterModel> characters, StartRunLobby lobby) =>
		WatcherRunRule.OfferedInLobby(lobby.GameMode == GameMode.Daily)
			? characters.Append(ModelDb.Character<Sts1Watcher>()).ToArray()
			: characters;
}
