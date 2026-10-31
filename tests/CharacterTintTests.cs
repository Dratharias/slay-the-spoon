using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class CharacterTintTests
{
	[Theory]
	[InlineData(TintMode.Off, 0, 0, 5, false)]
	[InlineData(TintMode.Overlay, 359, 100, 90, true)]
	[InlineData(TintMode.Dominant, 170, 70, 40, true)]
	public void Encoding_round_trips_within_the_network_bits(TintMode mode, int hue, int strength, int threshold, bool map)
	{
		var tint = new CharacterTint(mode, hue, strength, threshold, map);
		int code = tint.Encode();
		Assert.InRange(code, 0, (1 << CharacterTint.Bits) - 1);
		Assert.Equal(tint, CharacterTint.Decode(code));
	}

	[Fact]
	public void Values_are_clamped_and_hue_wraps()
	{
		var tint = new CharacterTint(TintMode.Overlay, -10, 150, 1, true).Clamped();
		Assert.Equal(350, tint.Hue);
		Assert.Equal(100, tint.Strength);
		Assert.Equal(CharacterTint.MinThreshold, tint.Threshold);
	}

	[Fact]
	public void The_silent_green_has_a_green_hue()
	{
		// Silent's map drawing color, 2F6729.
		float hue = TintMath.HueOf(0x2F / 255f, 0x67 / 255f, 0x29 / 255f);
		Assert.InRange(hue * 360f, 100f, 130f);
	}

	[Fact]
	public void Hue_distance_wraps_around_red()
	{
		Assert.Equal(0.1f, TintMath.HueDistance(0.95f, 0.05f), 4);
	}

	[Fact]
	public void Dominant_map_color_takes_the_chosen_hue_and_keeps_lightness()
	{
		var green = (0x2F / 255f, 0x67 / 255f, 0x29 / 255f);
		var color = TintMath.MapColor(green, new CharacterTint(TintMode.Dominant, 180, 100, 40, true));
		Assert.InRange(TintMath.HueOf(color.R, color.G, color.B) * 360f, 179f, 181f);
		Assert.Equal(Math.Max(green.Item1, Math.Max(green.Item2, green.Item3)), Math.Max(color.R, Math.Max(color.G, color.B)), 4);
	}

	[Fact]
	public void Map_color_is_untouched_when_off_or_not_on_the_map()
	{
		var color = (0.2f, 0.4f, 0.1f);
		Assert.Equal(color, TintMath.MapColor(color, CharacterTint.Default));
		Assert.Equal(color, TintMath.MapColor(color, new CharacterTint(TintMode.Overlay, 0, 50, 40, false)));
	}
}
