using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>StS1 Ogg playback follows the game's squared Master/Music sliders and ends with the run.</summary>
internal static class EndingAudio
{
	private static AudioStreamPlayer? _player;
	private static string? _track;
	internal static int Generation { get; private set; }

	internal static void Stop()
	{
		Generation++;
		if (GodotObject.IsInstanceValid(_player)) { _player!.Stop(); _player.QueueFree(); }
		_player = null;
		_track = null;
	}

	internal static void Play(string track, bool loop)
	{
		if (_track == track && GodotObject.IsInstanceValid(_player)) return;
		Stop();
		if (NGame.Instance is not { } game) return;
		var stream = ResourceLoader.Load<AudioStreamOggVorbis>($"res://SlaySpoon/audio/{track}.ogg");
		if (stream == null) throw new InvalidOperationException($"Missing Ending audio: {track}");
		stream.Loop = loop;
		_player = new AudioStreamPlayer { Stream = stream, Bus = "Master" };
		_track = track;
		game.AddChild(_player);
		UpdateVolume();
		_player.Play();
	}

	internal static void UpdateVolume(float? master = null, float? music = null)
	{
		if (!GodotObject.IsInstanceValid(_player)) return;
		var settings = SaveManager.Instance.SettingsSave;
		_player!.VolumeLinear = StingerVolume.Linear(master ?? settings.VolumeMaster, music ?? settings.VolumeBgm);
	}

	internal static bool UpdateRunMusic(NRunMusicController controller)
	{
		if (Act4Routing.CurrentRun() is not { Act: Sts1EndingAct } run) return true;
		if (run.CurrentRoom is { IsVictoryRoom: true }) return false;
		controller.StopMusic();
		Play(run.CurrentRoom is CombatRoom { Encounter: Sts1CorruptHeartBoss } ? "ending_heart" : "ending_act", true);
		return false;
	}
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
internal static class EndingStingerPatch
{
	private static void Prefix(out bool __state)
	{
		__state = false;
		try
		{
			var run = Act4Routing.CurrentRun();
			__state = run is { Act: Sts1EndingAct, CurrentRoom.IsVictoryRoom: false };
		}
		catch (Exception e) { SpoonLog.Error($"Ending stinger routing failed: {e}"); }
	}

	private static void Postfix(bool __state, ref Task __result)
	{
		if (__state) __result = EnterThenPlay(__result, EndingAudio.Generation);
	}

	private static async Task EnterThenPlay(Task entering, int generation)
	{
		await entering;
		try
		{
			// A return to the menu during the transition invalidates this pending playback.
			if (generation != EndingAudio.Generation || Act4Routing.CurrentRun()?.CurrentRoom is not { IsVictoryRoom: true }) return;
			NRunMusicController.Instance?.StopMusic();
			EndingAudio.Play("ending_stinger", false);
		}
		catch (Exception e) { SpoonLog.Error($"Ending stinger failed: {e}"); }
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenu))]
internal static class EndingAudioMenuPatch
{
	private static void Prefix() => EndingAudio.Stop();
}

[HarmonyPatch]
internal static class EndingMusicPatch
{
	[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateMusic))]
	[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateTrack), new Type[] { })]
	[HarmonyPrefix]
	private static bool Update(NRunMusicController __instance)
	{
		try { return EndingAudio.UpdateRunMusic(__instance); }
		catch (Exception e) { SpoonLog.Error($"Ending music failed: {e}"); return true; }
	}

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetMasterVol))]
	[HarmonyPostfix]
	private static void Master(float volume) => EndingAudio.UpdateVolume(master: volume);

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetBgmVol))]
	[HarmonyPostfix]
	private static void Music(float volume) => EndingAudio.UpdateVolume(music: volume);
}
