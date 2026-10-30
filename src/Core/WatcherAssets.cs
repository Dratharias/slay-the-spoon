namespace SlaySpoon.Core;

/// <summary>
/// The Watcher's static art in the PCK (cut from the StS1 jar by <c>make sts1-watcher-art</c>) and where each image's
/// ground point sits. Kept free of Godot types so tests can check every path against the repo.
/// </summary>
public static class WatcherAssets
{
	/// <summary>ModelDb id of <c>Sts1Watcher</c>; vanilla asset paths derive from its lowercase form.</summary>
	public const string CharacterId = "STS1_WATCHER";

	private const string Root = "res://SlaySpoon/images/";

	/// <summary>The idle pose with the eye closed (calm/wrath/divinity eye frames are exported for the stances).</summary>
	public const string Body = Root + "creatures/sts1_watcher/body.png";

	/// <summary>Body ground point (between the feet) in image pixels, from the top-left.</summary>
	public const float BodyOriginX = 171f, BodyOriginY = 274f;

	public const string Corpse = Root + "creatures/sts1_watcher/corpse.png";

	public const float CorpseOriginX = 256f, CorpseOriginY = 512f;

	/// <summary>Combat hover box relative to the ground point (x, y, width, height); intents and talk sit above it.</summary>
	public const float HitboxX = -120f, HitboxY = -270f, HitboxWidth = 240f, HitboxHeight = 270f;

	public const string TopBarIcon = Root + "ui/top_panel/character_icon_sts1_watcher.png";

	public const string TopBarIconOutline = Root + "ui/top_panel/character_icon_sts1_watcher_outline.png";

	public const string CharacterSelectIcon = Root + "packed/character_select/char_select_sts1_watcher.png";

	public const string CharacterSelectLockedIcon = Root + "packed/character_select/char_select_sts1_watcher_locked.png";

	public const string MapMarker = Root + "packed/map/icons/map_marker_sts1_watcher.png";

	/// <summary>Textures for the vanilla energy counter's Layer1..Layer5, in that order.</summary>
	public static readonly IReadOnlyList<string> OrbLayers = Enumerable.Range(1, 5)
		.Select(i => $"{Root}ui/combat/energy_counters/sts1_watcher/sts1_watcher_orb_layer_{i}.png")
		.ToArray();

	public static IReadOnlyList<string> ModImages { get; } = new[]
		{
			Body, Corpse, TopBarIcon, TopBarIconOutline, CharacterSelectIcon, CharacterSelectLockedIcon, MapMarker,
		}
		.Concat(OrbLayers)
		.ToArray();
}
