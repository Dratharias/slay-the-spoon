using System.Text.Json;
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

	[Fact(Skip = "Cards are ported in Tasks 7-10")]
	public void NoUnportedCards()
	{
		string[] unported = Directory.EnumerateFiles(Path.Combine(Repo, "src/Cards/Sts1"), "*.cs", SearchOption.AllDirectories)
			.Where(f => File.ReadAllText(f).Contains("NotImplementedException"))
			.Select(Path.GetFileName)
			.ToArray()!;
		Assert.Empty(unported);
	}
}
