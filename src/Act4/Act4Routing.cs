using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Spoon;

namespace SlaySpoon.Act4;

/// <summary>
/// Appends The Ending as the last act once the team holds all three keys (after the Spoon act when Spoon mode put
/// it there). From then on vanilla reads the act count:
/// the act 3 boss is an ordinary boss (rewards, no win time, its reward screen enters act 4) and the Heart is the
/// final boss (no rewards, win time, then the TheArchitect victory room and WinRun). The A10 second boss stays on
/// act 3, where vanilla put it at run start.
/// </summary>
/// <remarks>
/// Keys are only taken inside synced actions and hooks, so every co-op peer appends at the same logical point.
/// The act list is in no checksum; it is saved with the run and reloads through <c>ActModel.FromSave</c>.
/// </remarks>
public static class Act4Routing
{
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	internal static readonly Action<RunState, IReadOnlyList<ActModel>> SetActs =
		AccessTools.MethodDelegate<Action<RunState, IReadOnlyList<ActModel>>>(AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Acts)));

	private static bool _registered;

	/// <summary>Subscribes to key changes; ModEntry calls it once.</summary>
	public static void Register()
	{
		if (_registered) return;
		_registered = true;
		Act4Keys.Changed += OnKeysChanged;
	}

	/// <summary>The run RunManager is playing, or null between runs (for example while a save is being read).</summary>
	public static RunState? CurrentRun() => State(RunManager.Instance);

	/// <summary>Appends the Ending when <see cref="EndingRouteRule.ShouldAppend"/> allows it; true when it appended.</summary>
	public static bool EnsureEndingAct(RunState state)
	{
		SpoonAct4? act4 = SpoonAct4.Of(state);
		bool spoonPresent = state.Acts.Any(a => a is SpoonAct);
		if (!EndingRouteRule.ShouldAppend(act4 != null, act4?.Keys.HasAll ?? false, state.Acts.Count, spoonPresent)) return false;
		// A new list, as vanilla's debug act switch does: the old list may be enumerated elsewhere.
		SetActs(state, new List<ActModel>(state.Acts) { Sts1EndingAct.CreateForRun(state) });
		SpoonLog.Info("act4: The Ending appended");
		// A save taken on the act 3 boss's reward screen, after it counted as the final boss, carries its WinTime.
		RunManager manager = RunManager.Instance;
		if (EndingRouteRule.ShouldResetWinTime(true, manager.WinTime))
		{
			SpoonLog.Info($"act4: stale WinTime {manager.WinTime} cleared");
			manager.WinTime = 0;
		}
		return true;
	}

	private static void OnKeysChanged()
	{
		try
		{
			// Null while a save is being read: the saved-run postfix repairs that case.
			if (CurrentRun() is { } run) EnsureEndingAct(run);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"act4: routing after a key change failed: {e}");
		}
	}
}

/// <summary>Repairs a save taken between the last key and the append; a no-op when the save already has four acts.</summary>
[HarmonyPatch(typeof(RunManager), "InitializeSavedRun")]
internal static class EndingSavedRunPatch
{
	private static void Postfix()
	{
		try
		{
			if (Act4Routing.CurrentRun() is { } run) Act4Routing.EnsureEndingAct(run);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"act4: routing after a load failed: {e}");
		}
	}
}

/// <summary>Last guard before leaving an act: a run that holds all keys never treats act 3 as the last act.</summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
internal static class EndingEnterNextActPatch
{
	private static void Prefix()
	{
		try
		{
			if (Act4Routing.CurrentRun() is not { } run || !Act4Routing.EnsureEndingAct(run)) return;
			// Reaching here means the other callers missed the last key, so the act 3 boss counted as the final boss.
			// Its recorded win time would freeze the run clock (RunTime returns WinTime once set), and the Heart would
			// record that frozen time again.
			// EnsureEndingAct has already cleared the stale WinTime.
			SpoonLog.Error("act4: The Ending was appended only when leaving an act; the last boss may have given no rewards");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"act4: routing before the next act failed: {e}");
		}
	}
}
