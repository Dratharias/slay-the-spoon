using System.Text.Json;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>
/// The Watcher's Architect lines: vanilla builds each key as THE_ARCHITECT.talk.STS1_WATCHER.{dialogue}-{line}r.{ancient|char},
/// with a ".next" button on every line but the last; ArchitectDialoguePatch declares two dialogues of 3 and 2 lines.
/// </summary>
public class ArchitectDialogueTextTests
{
	private static readonly string Repo = FindRepo();

	private static string FindRepo()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir != null && !File.Exists(Path.Combine(dir.FullName, "mod", "SlaySpoon.json"))) dir = dir.Parent;
		return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
	}

	[Theory]
	[InlineData("eng")]
	[InlineData("fra")]
	public void EveryLineHasOneSpeakerAndEveryButLastANextButton(string lang)
	{
		var table = JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot", "SlaySpoon", "localization", lang, "ancients.json")))!;
		const string key = "THE_ARCHITECT.talk.STS1_WATCHER.";
		int[] lengths = { 3, 2 };
		var expected = new HashSet<string>();
		for (int d = 0; d < lengths.Length; d++)
		{
			for (int i = 0; i < lengths[d]; i++)
			{
				string line = $"{key}{d}-{i}r";
				Assert.True(table.ContainsKey(line + ".ancient") ^ table.ContainsKey(line + ".char"), line);
				expected.Add(table.ContainsKey(line + ".ancient") ? line + ".ancient" : line + ".char");
				if (i < lengths[d] - 1) expected.Add(line + ".next");
			}
		}
		Assert.Equal(expected.OrderBy(k => k), table.Keys.Where(k => k.StartsWith(key)).OrderBy(k => k));
	}
}
