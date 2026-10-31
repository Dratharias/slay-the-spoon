namespace SlaySpoon.Core;

/// <summary>Linear volume for a Godot-played sound so it follows the game's Master and Music sliders (0 to 1).</summary>
public static class StingerVolume
{
	/// <summary>The game squares each slider value before handing it to FMOD; the stinger does the same.</summary>
	public static float Linear(float master, float music)
	{
		float m = Math.Clamp(master, 0f, 1f);
		float b = Math.Clamp(music, 0f, 1f);
		return m * m * b * b;
	}
}
