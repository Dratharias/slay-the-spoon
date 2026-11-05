using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.Saves;

/// <summary>
/// Keeps the Watcher's modded-profile stats entry (created by any Custom run) from changing vanilla progress.
/// <c>UpdateEpochsPostRun</c> grants the Daily and Orobas epochs once <c>CharacterStats.Count</c> reaches the number
/// of vanilla characters: both reads count only vanilla entries. In a solo lobby the ascension limit reads the
/// character's <c>MaxAscension</c>, always 0 for her: for her it reads the best vanilla character's instead.
/// Nothing is written to either profile.
/// </summary>
internal static class WatcherVanillaGuardsPatch
{
	private static readonly MethodInfo GetCharacterStats = AccessTools.PropertyGetter(typeof(ProgressState), nameof(ProgressState.CharacterStats));
	private static readonly MethodInfo CountHelper = AccessTools.Method(typeof(WatcherVanillaGuardsPatch), nameof(VanillaCount));
	private static readonly MethodInfo GetMaxAscension = AccessTools.PropertyGetter(typeof(CharacterStats), nameof(CharacterStats.MaxAscension));
	private static readonly MethodInfo MaxHelper = AccessTools.Method(typeof(WatcherVanillaGuardsPatch), nameof(MaxAscensionFor));

	// Built on first use, once ModelDb is ready.
	private static readonly Lazy<string[]> VanillaIdList = new(() => new[]
	{
		ModelDb.GetId<Ironclad>().Entry, ModelDb.GetId<Silent>().Entry, ModelDb.GetId<Regent>().Entry,
		ModelDb.GetId<Necrobinder>().Entry, ModelDb.GetId<Defect>().Entry,
	});

	private static IEnumerable<string> VanillaIds() => VanillaIdList.Value;

	public static int VanillaCount(IReadOnlyDictionary<ModelId, CharacterStats> stats) =>
		WatcherProgress.VanillaStatsCount(stats.Keys.Select(id => id.Entry), VanillaIds());

	public static int MaxAscensionFor(CharacterStats stats)
	{
		string watcher = ModelDb.GetId<Sts1Watcher>().Entry;
		if (stats.Id.Entry != watcher)
		{
			return stats.MaxAscension;
		}
		int cap = WatcherAscension.LimitFor(stats.Id.Entry, stats.MaxAscension, watcher,
			SaveManager.Instance.Progress.CharacterStats.Select(pair => (pair.Key.Entry, pair.Value.MaxAscension)));
		// Logged once per value: the getter runs on every lobby refresh.
		if (cap != _loggedCap)
		{
			_loggedCap = cap;
			SpoonLog.Info($"watcher: ascension capped at {cap} (best vanilla character)");
		}
		return cap;
	}

	private static int _loggedCap = -1;

	[HarmonyPatch(typeof(ProgressSaveManager), "UpdateEpochsPostRun")]
	private static class EpochCount
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			CodeInstruction? previous = null;
			int rewritten = 0;
			foreach (CodeInstruction instruction in instructions)
			{
				if (previous != null && previous.operand is MethodInfo pm && pm == GetCharacterStats
					&& instruction.operand is MethodInfo { Name: "get_Count" })
				{
					rewritten++;
					// Keep any branch target or exception block that pointed at the replaced call.
					yield return new CodeInstruction(OpCodes.Call, CountHelper) { labels = instruction.labels, blocks = instruction.blocks };
				}
				else
				{
					yield return instruction;
				}
				previous = instruction;
			}
			if (rewritten != 2)
			{
				SpoonLog.Error($"watcher: rewrote {rewritten} of 2 CharacterStats.Count reads in UpdateEpochsPostRun; Watcher runs may grant epochs early");
			}
		}
	}

	[HarmonyPatch(typeof(StartRunLobby), "BeginRunLocally")]
	private static class BeginRun
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			ReplaceMaxAscension(instructions, "StartRunLobby.BeginRunLocally");
	}

	[HarmonyPatch(typeof(StartRunLobby), "SetSingleplayerAscensionAfterCharacterChanged")]
	private static class CharacterChanged
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			ReplaceMaxAscension(instructions, "StartRunLobby.SetSingleplayerAscensionAfterCharacterChanged");
	}

	/// <summary>
	/// The game-over screen indexes <c>CharacterStats</c> directly when saving badges. Her entry normally exists by then
	/// (the run's progress update creates it), but a missing one would throw on her first death or victory, so it is
	/// created first, in memory, in the modded profile only. Vanilla characters are untouched.
	/// </summary>
	[HarmonyPatch(typeof(NGameOverScreen), "SaveBadgesToProgress")]
	private static class GameOverStats
	{
		private static readonly AccessTools.FieldRef<NGameOverScreen, Player> LocalPlayer =
			AccessTools.FieldRefAccess<NGameOverScreen, Player>("_localPlayer");

		private static void Prefix(NGameOverScreen __instance)
		{
			try
			{
				if (LocalPlayer(__instance)?.Character is Sts1Watcher watcher)
					SaveManager.Instance.Progress.GetOrCreateCharacterStats(watcher.Id);
			}
			catch (Exception e)
			{
				SpoonLog.Error($"watcher: game-over stats guard failed: {e}");
			}
		}
	}

	private static IEnumerable<CodeInstruction> ReplaceMaxAscension(IEnumerable<CodeInstruction> instructions, string where)
	{
		int rewritten = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.operand is MethodInfo method && method == GetMaxAscension)
			{
				rewritten++;
				yield return new CodeInstruction(OpCodes.Call, MaxHelper) { labels = instruction.labels, blocks = instruction.blocks };
			}
			else
			{
				yield return instruction;
			}
		}
		if (rewritten == 0)
		{
			SpoonLog.Error($"watcher: no CharacterStats.MaxAscension read in {where}; a solo Watcher run stays at ascension 0");
		}
	}
}
