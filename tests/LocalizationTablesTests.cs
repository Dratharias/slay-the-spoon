using Xunit;

namespace SlaySpoon.Tests;

/// <summary>
/// The game only merges mod localization files into tables it already loads, so a file with any other name is never read
/// and its LocString lookups throw.
/// </summary>
public class LocalizationTablesTests
{
	private static readonly string Repo = FindRepo();

	/// <summary>Vanilla table names, from the game PCK (localization/eng/*.json).</summary>
	private static readonly HashSet<string> VanillaTables =
	[
		"achievements",
		"acts",
		"afflictions",
		"ancients",
		"ascension",
		"badges",
		"bestiary",
		"card_keywords",
		"card_library",
		"card_reward_ui",
		"card_selection",
		"cards",
		"characters",
		"combat_messages",
		"credits",
		"enchantments",
		"encounters",
		"epochs",
		"eras",
		"events",
		"extensions",
		"ftues",
		"game_modes",
		"game_over_screen",
		"gameplay_ui",
		"inspect_relic_screen",
		"intents",
		"main_menu_ui",
		"map",
		"merchant_room",
		"modifiers",
		"monsters",
		"orbs",
		"potion_lab",
		"potions",
		"powers",
		"relic_collection",
		"relics",
		"rest_site_ui",
		"rich_presence",
		"run_history",
		"settings_ui",
		"static_hover_tips",
		"stats_screen",
		"timeline",
		"vfx",
	];

	private static string FindRepo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile")))
		{
			dir = Path.GetDirectoryName(dir);
		}
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void EveryModTable_IsAVanillaTable(string lang)
	{
		string[] unknown = Directory.EnumerateFiles(Path.Combine(Repo, "godot/SlaySpoon/localization", lang), "*.json")
			.Select(Path.GetFileNameWithoutExtension)
			.Where(n => !VanillaTables.Contains(n!))
			.ToArray()!;
		Assert.True(unknown.Length == 0, $"Not vanilla tables (never loaded): {string.Join(", ", unknown)}");
	}
}
