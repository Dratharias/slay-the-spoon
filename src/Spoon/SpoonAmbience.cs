using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Act4;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// A light restaurant ambience under the Spoon act's music (owner request 2026-11-16): a looped public-domain recording
/// ("Restaurant Ambience", stephan, PDSounds 274, via Wikimedia Commons), set like a sound effect and kept low.
/// It starts when the run's music updates in the Spoon act and stops anywhere else.
/// </summary>
internal static class SpoonAmbience
{
	private const string Path = "res://SlaySpoon/audio/spoon/restaurant.ogg";
	private static AudioStreamPlayer? _player;

	internal static void Update()
	{
		if (Act4Routing.CurrentRun() is { Act: SpoonAct }) Play();
		else Stop();
	}

	private static void Play()
	{
		if (GodotObject.IsInstanceValid(_player)) return;
		if (NGame.Instance is not { } game) return;
		var stream = ResourceLoader.Load<AudioStreamOggVorbis>(Path);
		if (stream == null) throw new InvalidOperationException($"Missing Spoon ambience: {Path}");
		stream.Loop = true;
		_player = new AudioStreamPlayer { Stream = stream, Bus = "Master", VolumeLinear = 0f };
		game.AddChild(_player);
		_player.Play();
		_player.CreateTween().TweenProperty(_player, "volume_linear", Target(), SpoonAmbienceRule.FadeIn);
	}

	internal static void Stop()
	{
		if (GodotObject.IsInstanceValid(_player)) { _player!.Stop(); _player.QueueFree(); }
		_player = null;
	}

	private static float Target(float? master = null, float? sfx = null)
	{
		var settings = SaveManager.Instance.SettingsSave;
		return SpoonAmbienceRule.Volume(master ?? settings.VolumeMaster, sfx ?? settings.VolumeSfx);
	}

	internal static void UpdateVolume(float? master = null, float? sfx = null)
	{
		if (GodotObject.IsInstanceValid(_player)) _player!.VolumeLinear = Target(master, sfx);
	}
}

[HarmonyPatch]
internal static class SpoonAmbiencePatch
{
	[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateMusic))]
	[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateTrack), new Type[] { })]
	[HarmonyPostfix]
	private static void AfterMusic()
	{
		try { SpoonAmbience.Update(); }
		catch (Exception e) { SpoonLog.Error($"spoon ambience failed: {e}"); }
	}

	[HarmonyPatch(typeof(NGame), nameof(NGame.ReturnToMainMenu))]
	[HarmonyPrefix]
	private static void ToMenu() => SpoonAmbience.Stop();

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetMasterVol))]
	[HarmonyPostfix]
	private static void Master(float volume) => SpoonAmbience.UpdateVolume(master: volume);

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetSfxVol))]
	[HarmonyPostfix]
	private static void Sfx(float volume) => SpoonAmbience.UpdateVolume(sfx: volume);
}
