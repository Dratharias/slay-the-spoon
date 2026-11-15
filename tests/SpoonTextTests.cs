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
		foreach (string id in new[] { "SPOON_SPOONFUL", "SPOON_SPOON_FEED" })
			foreach (string part in new[] { "title", "description" })
				Assert.True(cards.TryGetValue($"{id}.{part}", out string? v) && v.Length > 0, $"{lang} cards {id}.{part}");
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
}
