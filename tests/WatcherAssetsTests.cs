using System.Text.Json;
using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>The Watcher shell: every image path the character points at ships in the PCK, and its text is complete.</summary>
public class WatcherAssetsTests
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

	private static Dictionary<string, string> Table(string lang, string table) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, table + ".json")))!;

	[Fact]
	public void CharacterId_MatchesTheClassName()
	{
		Assert.Equal(WatcherAssets.CharacterId, ModelIds.FromClassName("Sts1Watcher"));
	}

	[Fact]
	public void EveryModImagePath_Exists()
	{
		Assert.NotEmpty(WatcherAssets.ModImages);
		foreach (string path in WatcherAssets.ModImages)
		{
			Assert.StartsWith("res://SlaySpoon/", path);
			string file = Path.Combine(Repo, "godot", path["res://".Length..]);
			Assert.True(File.Exists(file), "missing " + file);
		}
	}

	[Fact]
	public void OrbLayers_AreTheFiveEnergyCounterLayers()
	{
		Assert.Equal(5, WatcherAssets.OrbLayers.Count);
		Assert.All(WatcherAssets.OrbLayers, p => Assert.Contains(p, WatcherAssets.ModImages));
	}

	/// <summary>Keys the game reads from the characters table for a character (CharacterModel, events, co-op banter).</summary>
	public static readonly string[] CharacterKeys =
	{
		"title", "titleObject", "description", "pronounObject", "possessiveAdjective", "pronounPossessive",
		"pronounSubject", "cardsModifierTitle", "cardsModifierDescription", "eventDeathPrevention", "unlockText",
		"goldMonologue", "aromaPrinciple", "banter.alive.endTurnPing", "banter.dead.endTurnPing",
	};

	[Fact]
	public void CharacterTexts_AreComplete()
	{
		foreach (string lang in new[] { "eng", "fra" })
		{
			Dictionary<string, string> characters = Table(lang, "characters");
			foreach (string key in CharacterKeys)
			{
				Assert.False(string.IsNullOrWhiteSpace(characters.GetValueOrDefault($"{WatcherAssets.CharacterId}.{key}")),
					$"{lang} {key}");
			}
		}
	}

	[Fact]
	public void StarterRelicTexts_AreComplete()
	{
		string id = ModelIds.FromClassName("Sts1PureWater");
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
