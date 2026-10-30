using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Act4;

/// <summary>
/// Plays StS1's ending stinger once when the Heart's act hands over to the Architect (the victory room).
/// Vanilla <c>EnterNextAct</c> already does the rest: with the Ending act last, it enters the Architect event
/// room and a second call wins the run. The Architect never reads the act index, so nothing else is patched.
/// </summary>
/// <remarks>
/// The stinger is a plain Godot player on the Master bus, next to the game's FMOD music: the two mix in the
/// OS, and it ignores the game's volume sliders. A run resumed inside the victory room does not replay it,
/// because only the call that enters the room triggers it.
/// </remarks>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
internal static class EndingStingerPatch
{
	private const string StingerPath = "res://SlaySpoon/audio/ending_stinger.ogg";

	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	/// <summary>True when this call leaves the Ending act for the Architect (not the call that wins the run).</summary>
	private static void Prefix(out bool __state)
	{
		RunState? run = State(RunManager.Instance);
		__state = run != null
			&& run.Acts.Count > 0
			&& run.Acts[^1] is Sts1EndingAct
			&& run.CurrentActIndex >= run.Acts.Count - 1
			&& run.CurrentRoom is { IsVictoryRoom: false };
	}

	private static void Postfix(bool __state, ref Task __result)
	{
		if (__state) __result = EnterThenPlay(__result);
	}

	private static async Task EnterThenPlay(Task entering)
	{
		await entering;
		try
		{
			Play();
		}
		catch (Exception e)
		{
			SpoonLog.Error($"Ending stinger failed: {e.Message}");
		}
	}

	private static void Play()
	{
		if (NGame.Instance is not { } game) return;
		if (ResourceLoader.Load<AudioStream>(StingerPath) is not { } stream)
		{
			SpoonLog.Error($"Ending stinger missing: {StingerPath}");
			return;
		}
		var player = new AudioStreamPlayer { Stream = stream, Bus = "Master" };
		player.Finished += player.QueueFree;
		game.AddChild(player);
		player.Play();
		SpoonLog.Info("act4: ending stinger played");
	}
}
