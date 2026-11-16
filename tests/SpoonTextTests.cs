using System.Text.Json;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>The Spoon mode's texts exist in both languages (keys are the ModelDb id entries of the classes).</summary>
public class SpoonTextTests
{
	private static string Repo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile"))) dir = Path.GetDirectoryName(dir);
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	private static Dictionary<string, string> Table(string lang, string table) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo(), "godot/SlaySpoon/localization", lang, table + ".json")))!;

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void Cards_have_text(string lang)
	{
		Dictionary<string, string> cards = Table(lang, "cards");
		foreach (string id in new[] { "SPOON_SPOONFUL", "SPOON_TEASPOON", "SPOON_WOODEN_SPOON", "SPOON_SKIMMER", "SPOON_MELON_BALLER", "SPOON_STOLEN_SPOONFUL" })
			foreach (string part in new[] { "title", "description" })
				Assert.True(cards.TryGetValue($"{id}.{part}", out string? v) && v.Length > 0, $"{lang} cards {id}.{part}");
	}

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void Powers_have_text(string lang)
	{
		Dictionary<string, string> powers = Table(lang, "powers");
		foreach (string id in new[] { "SPOON_APPETITE_POWER", "SPOON_SATED_POWER", "SPOON_SKIMMED_POWER" })
			foreach (string part in new[] { "title", "description", "smartDescription" })
				Assert.True(powers.TryGetValue($"{id}.{part}", out string? v) && v.Length > 0, $"{lang} powers {id}.{part}");
	}

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void Modifier_has_text(string lang)
	{
		Dictionary<string, string> mods = Table(lang, "modifiers");
		foreach (string part in new[] { "title", "description" })
			Assert.True(mods.TryGetValue($"SPOON_SPOON.{part}", out string? v) && v.Length > 0, $"{lang} modifiers SPOON_SPOON.{part}");
	}

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void Boss_has_text(string lang)
	{
		Dictionary<string, string> monsters = Table(lang, "monsters");
		foreach (string key in new[] { "SPOON_MONSTER.name", "SPOON_MONSTER.moves.BOUCHEE.title", "SPOON_MONSTER.moves.LADLE.title",
			"SPOON_MONSTER.moves.SET_TABLE.title", "SPOON_MONSTER.moves.BANQUET.title" })
			Assert.True(monsters.TryGetValue(key, out string? v) && v.Length > 0, $"{lang} monsters {key}");
		Dictionary<string, string> encounters = Table(lang, "encounters");
		foreach (string key in new[] { "SPOON_BOSS.title", "SPOON_BOSS.loss" })
			Assert.True(encounters.TryGetValue(key, out string? v) && v.Length > 0, $"{lang} encounters {key}");
	}

	[Fact]
	public void Spoon_art_exists()
	{
		foreach (string path in new[]
		{
			"images/spoon/spoon.png", "images/spoon/spoon_boss.png", "images/spoon/spoon_boss_outline.png",
			"images/cards/spoon/spoon_spoonful.png",
			"images/cards/spoon/spoon_teaspoon.png", "images/cards/spoon/spoon_wooden_spoon.png",
			"images/cards/spoon/spoon_skimmer.png", "images/cards/spoon/spoon_melon_baller.png",
			"images/cards/spoon/spoon_stolen_spoonful.png",
		})
			Assert.True(File.Exists(Path.Combine(Repo(), "godot/SlaySpoon", path)), path);
	}
}
