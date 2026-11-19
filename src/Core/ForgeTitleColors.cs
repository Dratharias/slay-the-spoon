namespace SlaySpoon.Core;

/// <summary>The rarity banner a card title sits on, for <see cref="ForgeTitleColors"/>.</summary>
public enum TitleBanner
{
	Common,
	Uncommon,
	Rare,
	Other,
}

/// <summary>A title's fill and outline as RRGGBB hex, and whether it shimmers.</summary>
public readonly record struct TitleColors(string Fill, string Outline, bool Shimmer);

/// <summary>
/// Forged card titles climb a rarity ladder (owner, 2026-11-19): +1 keeps the vanilla green, then yellow-green, orange,
/// red, magenta, violet, blue, cyan, and a shimmering diamond at +9, which also wears the Ancient frame. Each color has
/// a dark outline of its own hue; a color close to its banner (warm on the gold Rare banner, blue on the blue Uncommon
/// one) gets a near-black outline and a lighter fill so it stays readable. The frame turns Ancient by the same steps.
/// </summary>
public static class ForgeTitleColors
{
	public const int DiamondLevel = 9;

	private static readonly TitleColors[] Ladder =
	[
		new("C8FF3C", "3F5E00", false), // +2 yellow-green
		new("FFA43A", "6E3300", false), // +3 orange
		new("FF5A4A", "6E0F0A", false), // +4 red
		new("FF66D6", "650E52", false), // +5 magenta
		new("BE94FF", "3B1C7A", false), // +6 violet
		new("6FA8FF", "0F2D7A", false), // +7 blue
		new("5CF2FF", "005463", false), // +8 cyan
		new("F2FDFF", "1F6E8A", true), // +9 diamond
	];

	/// <summary>The colors for a forged title, or null below +2 (the game's own colors stay).</summary>
	public static TitleColors? For(int level, TitleBanner banner)
	{
		if (level < 2) return null;
		var colors = Ladder[Math.Min(level, DiamondLevel) - 2];
		return Clashes(level, banner) ? colors with { Fill = Lighter(level, colors.Fill), Outline = "140C00" } : colors;
	}

	/// <summary>How far the frame has turned Ancient: 0 below +2, then an even step per level, fully Ancient at +9.</summary>
	public static float AncientBlend(int level) => level < 2 ? 0f : Math.Min(1f, (level - 1) / (float)(DiamondLevel - 1));

	/// <summary>The Ancient banner replaces the ordinary one from this level (the banners swap; they never crossfade).</summary>
	public const int AncientBannerLevel = 6;

	public static bool AncientBanner(int level) => level >= AncientBannerLevel;

	/// <summary>The strength of the gloss sweeping the frame: none below +2, rising to full at +9.</summary>
	public static float Gloss(int level) => level < 2 ? 0f : 0.15f + 0.55f * AncientBlend(level);

	/// <summary>Colors too close to the banner under them: warm on gold, blue on blue.</summary>
	public static bool Clashes(int level, TitleBanner banner) => banner switch
	{
		TitleBanner.Rare => level is 2 or 3,
		TitleBanner.Uncommon => level is 7 or 8,
		_ => false,
	};

	private static string Lighter(int level, string fill) => level switch
	{
		2 => "E2FF8A",
		3 => "FFC98A",
		7 => "A9CBFF",
		8 => "B5FAFF",
		_ => fill,
	};
}
