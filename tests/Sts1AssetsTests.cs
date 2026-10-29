using System.Text.Json;
using System.Text.RegularExpressions;
using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Every StS1 card class ships its text in both languages and its portrait.</summary>
public class Sts1AssetsTests
{
	private static readonly string Repo = FindRepo();

	private static string FindRepo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile")))
		{
			dir = Path.GetDirectoryName(dir);
		}
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	private static IEnumerable<string> CardFiles(string character) =>
		Directory.EnumerateFiles(Path.Combine(Repo, "src/Cards/Sts1", character), "Sts1*.cs");

	private static Dictionary<string, string> Table(string lang, string table) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, table + ".json")))!;

	[Fact]
	public void Sts1Assets_AreComplete()
	{
		string[] files = CardFiles("Ironclad").ToArray();
		Assert.NotEmpty(files);
		List<string> problems = new();
		foreach (string lang in new[] { "eng", "fra" })
		{
			Dictionary<string, string> cards = Table(lang, "cards");
			foreach (string file in files)
			{
				string id = ModelIds.FromClassName(Path.GetFileNameWithoutExtension(file));
				foreach (string key in new[] { id + ".title", id + ".description" })
				{
					if (!cards.TryGetValue(key, out string? text) || string.IsNullOrWhiteSpace(text))
					{
						problems.Add($"{lang}: {key}");
					}
				}
			}
		}
		foreach (string file in files)
		{
			string png = Path.Combine(Repo, "godot/SlaySpoon/images/cards/ironclad",
				ModelIds.FromClassName(Path.GetFileNameWithoutExtension(file)).ToLowerInvariant() + ".png");
			if (!File.Exists(png))
			{
				problems.Add("missing " + png);
			}
		}
		Assert.Empty(problems);
	}

	[Fact]
	public void NoUnportedCards()
	{
		string[] unported = Directory.EnumerateFiles(Path.Combine(Repo, "src/Cards/Sts1"), "*.cs", SearchOption.AllDirectories)
			.Where(f => File.ReadAllText(f).Contains("NotImplementedException"))
			.Select(Path.GetFileName)
			.ToArray()!;
		Assert.Empty(unported);
	}

	// Placeholders the game fills itself rather than from the card's own vars.
	private static readonly HashSet<string> BuiltInPlaceholders = new() { "IfUpgraded", "energyPrefix" };

	/// <summary>A {Name:...} placeholder with no matching card var throws when the card text is shown.</summary>
	[Fact]
	public void CardTextPlaceholders_MatchCardVars()
	{
		var problems = new List<string>();
		foreach (string file in CardFiles("Ironclad"))
		{
			string source = File.ReadAllText(file);
			var vars = new HashSet<string>(BuiltInPlaceholders);
			foreach (Match m in Regex.Matches(source, @"new (DamageVar|BlockVar|EnergyVar|CardsVar|RepeatVar)\("))
			{
				vars.Add(m.Groups[1].Value[..^3]);
			}
			foreach (Match m in Regex.Matches(source, @"new PowerVar<(\w+)>\("))
			{
				vars.Add(m.Groups[1].Value);
			}
			foreach (Match m in Regex.Matches(source, @"new DynamicVar\(""(\w+)"""))
			{
				vars.Add(m.Groups[1].Value);
			}
			// Var names held in a constant, as in new DynamicVar(IncreaseKey, 5m).
			foreach (Match m in Regex.Matches(source, @"const string \w+Key = ""(\w+)"""))
			{
				vars.Add(m.Groups[1].Value);
			}
			string key = ModelIds.FromClassName(Path.GetFileNameWithoutExtension(file));
			foreach (string lang in new[] { "eng", "fra" })
			{
				string text = Table(lang, "cards").GetValueOrDefault(key + ".description", "");
				foreach (Match m in Regex.Matches(text, @"\{(\w+):"))
				{
					if (!vars.Contains(m.Groups[1].Value))
					{
						problems.Add($"{lang} {key}: {{{m.Groups[1].Value}}}");
					}
				}
			}
		}
		Assert.Empty(problems);
	}

	[Fact]
	public void RelicTexts_AreComplete()
	{
		string id = ModelIds.FromClassName("SpoonSpireSouvenir");
		foreach (string lang in new[] { "eng", "fra" })
		{
			Dictionary<string, string> relics = Table(lang, "relics");
			foreach (string key in new[] { "title", "description", "flavor" })
			{
				Assert.False(string.IsNullOrWhiteSpace(relics.GetValueOrDefault($"{id}.{key}")), $"{lang} {key}");
			}
		}
	}
}
