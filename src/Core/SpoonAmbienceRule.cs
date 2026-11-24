namespace SlaySpoon.Core;

/// <summary>Level of the Spoon act's restaurant ambience (owner request 2026-11-16): a sound effect, kept well under the music.</summary>
public static class SpoonAmbienceRule
{
	/// <summary>Linear gain on top of the SFX sliders, about −9 dB.</summary>
	public const float Level = 0.35f;

	/// <summary>Seconds of fade-in when the ambience starts.</summary>
	public const float FadeIn = 2f;

	/// <summary>The game's squared Master and SFX sliders, times <see cref="Level"/>.</summary>
	public static float Volume(float master, float sfx) => StingerVolume.Linear(master, sfx) * Level;
}
