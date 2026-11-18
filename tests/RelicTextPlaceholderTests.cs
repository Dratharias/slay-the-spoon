using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Relic texts: English and French use the same placeholders, and the Cursed Deck relics only use vars they declare.</summary>
public class RelicTextPlaceholderTests
{
	private static readonly string Repo = FindRepo();

	private static string FindRepo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile"))) dir = Path.GetDirectoryName(dir);
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	private static Dictionary<string, string> Table(string lang) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, "relics.json")))!;

	private static HashSet<string> Placeholders(string text) =>
		Regex.Matches(text, @"\{(\w+)[:}]").Select(m => m.Groups[1].Value).ToHashSet();

	[Fact]
	public void EnglishAndFrench_UseTheSamePlaceholders()
	{
		var en = Table("eng");
		var fr = Table("fra");
		List<string> problems = new();
		foreach (var (key, text) in en)
		{
			if (!fr.TryGetValue(key, out var frText)) continue;
			if (!Placeholders(text).SetEquals(Placeholders(frText))) problems.Add(key);
		}
		Assert.True(problems.Count == 0, "placeholder mismatch: " + string.Join(", ", problems));
	}

	[Fact]
	public void CursedDeckRelics_OnlyUseDeclaredVars()
	{
		var declared = new Dictionary<string, string[]>
		{
			["SPOON_BLOOD_PACT"] = new[] { "Block" },
			["SPOON_PIN_DOLL"] = new[] { "Curses", "StrengthPower" },
			["SPOON_RELIQUARY"] = new[] { "Gold" },
		};
		foreach (var lang in new[] { "eng", "fra" })
		{
			var table = Table(lang);
			foreach (var (id, vars) in declared)
			{
				Assert.True(table.TryGetValue(id + ".description", out var text), id + " has no text in " + lang);
				Assert.True(Placeholders(text!).IsSubsetOf(vars), $"{id} ({lang}) uses an undeclared placeholder");
			}
		}
	}
}
