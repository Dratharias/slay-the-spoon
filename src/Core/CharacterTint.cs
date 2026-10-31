namespace SlaySpoon.Core;

/// <summary>How a player's character color is changed.</summary>
public enum TintMode
{
	Off,
	/// <summary>A flat color laid over the whole character at <see cref="CharacterTint.Strength"/> opacity.</summary>
	Overlay,
	/// <summary>Only the pixels near the character's dominant hue turn to the chosen hue.</summary>
	Dominant,
}

/// <summary>
/// A player's chosen character color, set locally and shared with the other players. The dominant hue is the
/// character's own map drawing color, which the game picks to stand for the character.
/// </summary>
public readonly record struct CharacterTint(TintMode Mode, int Hue, int Strength, int Threshold, bool Map)
{
	public const int MaxHue = 359;
	public const int MaxStrength = 100;
	public const int MinThreshold = 5;
	public const int MaxThreshold = 90;

	public static CharacterTint Default => new(TintMode.Off, 180, 20, 40, true);

	public bool Active => Mode != TintMode.Off && Strength > 0;

	public CharacterTint Clamped() => this with
	{
		Mode = Enum.IsDefined(Mode) ? Mode : TintMode.Off,
		Hue = ((Hue % 360) + 360) % 360,
		Strength = Math.Clamp(Strength, 0, MaxStrength),
		Threshold = Math.Clamp(Threshold, MinThreshold, MaxThreshold),
	};

	/// <summary>Mode (2 bits), map (1), strength (7), threshold (7), hue (9): 26 bits.</summary>
	public int Encode()
	{
		var t = Clamped();
		return (int)t.Mode | (t.Map ? 1 << 2 : 0) | t.Strength << 3 | t.Threshold << 10 | t.Hue << 17;
	}

	public static CharacterTint Decode(int code) => new CharacterTint((TintMode)(code & 3), (code >> 17) & 511,
		(code >> 3) & 127, (code >> 10) & 127, (code & 4) != 0).Clamped();

	public const int Bits = 26;
}

/// <summary>Color maths shared by the shader parameters and the map drawing color, on (r, g, b) in 0..1.</summary>
public static class TintMath
{
	/// <summary>Hue in 0..1 of an RGB color (0 for greys).</summary>
	public static float HueOf(float r, float g, float b)
	{
		float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
		if (d <= 1e-6f) return 0f;
		float h = max == r ? (g - b) / d % 6f : max == g ? (b - r) / d + 2f : (r - g) / d + 4f;
		h /= 6f;
		return h < 0 ? h + 1f : h;
	}

	/// <summary>The fully saturated color of hue <paramref name="hue"/> (0..1).</summary>
	public static (float R, float G, float B) Pure(float hue)
	{
		float h = (hue % 1f + 1f) % 1f * 6f;
		float x = 1f - Math.Abs(h % 2f - 1f);
		return (int)h switch
		{
			0 => (1f, x, 0f),
			1 => (x, 1f, 0f),
			2 => (0f, 1f, x),
			3 => (0f, x, 1f),
			4 => (x, 0f, 1f),
			_ => (1f, 0f, x),
		};
	}

	/// <summary>Shortest distance between two hues on the 0..1 wheel (0..0.5).</summary>
	public static float HueDistance(float a, float b)
	{
		float d = Math.Abs(a - b) % 1f;
		return Math.Min(d, 1f - d);
	}

	/// <summary>The map drawing color: overlay blends toward the chosen hue; dominant keeps lightness, takes the hue.</summary>
	public static (float R, float G, float B) MapColor((float R, float G, float B) color, CharacterTint tint)
	{
		if (!tint.Active || !tint.Map) return color;
		float s = tint.Strength / 100f;
		var target = Pure(tint.Hue / 360f);
		if (tint.Mode == TintMode.Overlay)
		{
			return (Lerp(color.R, target.R, s), Lerp(color.G, target.G, s), Lerp(color.B, target.B, s));
		}
		// Rebuild the color at the chosen hue with the same max/min (value and saturation), then blend by strength.
		float max = Math.Max(color.R, Math.Max(color.G, color.B)), min = Math.Min(color.R, Math.Min(color.G, color.B));
		var shifted = (min + target.R * (max - min), min + target.G * (max - min), min + target.B * (max - min));
		return (Lerp(color.R, shifted.Item1, s), Lerp(color.G, shifted.Item2, s), Lerp(color.B, shifted.Item3, s));
	}

	private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
