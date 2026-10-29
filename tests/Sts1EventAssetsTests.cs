using System.Text.Json;
using System.Text.RegularExpressions;
using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Every StS1 event ships its portrait and, in both languages, every text key its code looks up.</summary>
public class Sts1EventAssetsTests
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

	private static string[] EventFiles() =>
		Directory.EnumerateFiles(Path.Combine(Repo, "src/Events"), "Sts1*.cs")
			.Where(f => !f.EndsWith("Sts1Event.cs", StringComparison.Ordinal))
			.ToArray();

	private static Dictionary<string, string> Table(string lang, string table) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, table + ".json")))!;

	[Fact]
	public void EveryEventKind_HasAClass()
	{
		string[] ids = EventFiles().Select(f => ModelIds.FromClassName(Path.GetFileNameWithoutExtension(f))).ToArray();
		foreach (Sts1EventKind kind in Enum.GetValues<Sts1EventKind>())
		{
			Assert.Contains(ModelIds.FromClassName("Sts1" + kind), ids);
		}
	}

	[Fact]
	public void Events_HaveTextAndPortrait()
	{
		List<string> problems = new();
		foreach (string file in EventFiles())
		{
			string id = ModelIds.FromClassName(Path.GetFileNameWithoutExtension(file));
			string portrait = Path.Combine(Repo, "godot/SlaySpoon/images/events", id.ToLowerInvariant() + ".png");
			if (!File.Exists(portrait))
			{
				problems.Add("portrait: " + portrait);
			}
			string code = File.ReadAllText(file);
			List<string> keys = new() { id + ".title", id + ".pages.INITIAL.description" };
			foreach (Match m in Regex.Matches(code, "\"(" + Regex.Escape(id) + @"\.pages\.[A-Z_.]+)"""))
			{
				string key = m.Groups[1].Value;
				if (key.Contains(".options."))
				{
					keys.Add(key + ".title");
					keys.Add(key + ".description");
				}
				else
				{
					keys.Add(key);
				}
			}
			foreach (string lang in new[] { "eng", "fra" })
			{
				Dictionary<string, string> events = Table(lang, "events");
				problems.AddRange(keys.Where(k => !events.TryGetValue(k, out string? text) || string.IsNullOrWhiteSpace(text))
					.Select(k => $"{lang}: {k}"));
			}
		}
		Assert.Empty(problems);
	}
}
