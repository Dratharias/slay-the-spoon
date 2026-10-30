using System.Text.Json;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>Every Act 4 ending monster and encounter key a vanilla monster or encounter has, in both languages.</summary>
public class EndingTextTests
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

	private static Dictionary<string, string> Read(string lang, string table) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, table + ".json")))!;

	[Theory]
	[InlineData("monsters", "STS1_SPIRE_SHIELD.name")]
	[InlineData("monsters", "STS1_SPIRE_SHIELD.moves.BASH.title")]
	[InlineData("monsters", "STS1_SPIRE_SHIELD.moves.FORTIFY.title")]
	[InlineData("monsters", "STS1_SPIRE_SHIELD.moves.SMASH.title")]
	[InlineData("monsters", "STS1_SPIRE_SPEAR.name")]
	[InlineData("monsters", "STS1_SPIRE_SPEAR.moves.BURN_STRIKE.title")]
	[InlineData("monsters", "STS1_SPIRE_SPEAR.moves.PIERCER.title")]
	[InlineData("monsters", "STS1_SPIRE_SPEAR.moves.SKEWER.title")]
	[InlineData("encounters", "STS1_SHIELD_AND_SPEAR_ELITE.title")]
	[InlineData("encounters", "STS1_SHIELD_AND_SPEAR_ELITE.loss")]
	[InlineData("monsters", "STS1_CORRUPT_HEART.name")]
	[InlineData("monsters", "STS1_CORRUPT_HEART.moves.DEBILITATE.title")]
	[InlineData("monsters", "STS1_CORRUPT_HEART.moves.BLOOD_SHOTS.title")]
	[InlineData("monsters", "STS1_CORRUPT_HEART.moves.ECHO.title")]
	[InlineData("monsters", "STS1_CORRUPT_HEART.moves.BUFF.title")]
	[InlineData("encounters", "STS1_CORRUPT_HEART_BOSS.title")]
	[InlineData("encounters", "STS1_CORRUPT_HEART_BOSS.loss")]
	public void EndingTextExistsInBothLanguages(string table, string key)
	{
		foreach (string lang in new[] { "eng", "fra" })
		{
			Dictionary<string, string> text = Read(lang, table);
			Assert.True(text.TryGetValue(key, out string? value), $"{lang}/{table}.json has no {key}");
			Assert.False(string.IsNullOrWhiteSpace(value), $"{lang}/{table}.json: {key} is empty");
		}
	}
}
