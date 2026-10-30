using System.Text.Json;
using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Every power class the Watcher's Power cards apply has complete English and French text.</summary>
public class WatcherPowerTextTests
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

	[Fact]
	public void EveryWatcherPowerClass_HasTitleAndDescriptionsInBothLanguages()
	{
		// Powers that extend a vanilla power (Duality) keep its text.
		string[] files = Directory.GetFiles(Path.Combine(Repo, "src/Watcher/Powers"), "Sts1*Power.cs")
			.Where(file => File.ReadAllText(file).Contains(": PowerModel") || File.ReadAllText(file).Contains(", PowerModel")).ToArray();
		Assert.NotEmpty(files);
		foreach (string lang in new[] { "eng", "fra" })
		{
			var table = JsonSerializer.Deserialize<Dictionary<string, string>>(
				File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, "powers.json")))!;
			foreach (string file in files)
			{
				string id = ModelIds.FromClassName(Path.GetFileNameWithoutExtension(file));
				foreach (string key in new[] { "title", "description", "smartDescription" })
				{
					Assert.False(string.IsNullOrWhiteSpace(table.GetValueOrDefault($"{id}.{key}")), $"{lang} {id}.{key}");
				}
			}
		}
	}
}
