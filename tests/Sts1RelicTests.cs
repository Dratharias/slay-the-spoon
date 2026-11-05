using System.Text.Json;
using System.Text.RegularExpressions;
using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

/// <summary>The StS1 character and boss relics: their rules, and every relic ships art and text in both languages.</summary>
public class Sts1RelicTests
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

	private static Dictionary<string, string> Table(string lang) =>
		JsonSerializer.Deserialize<Dictionary<string, string>>(
			File.ReadAllText(Path.Combine(Repo, "godot/SlaySpoon/localization", lang, "relics.json")))!;

	/// <summary>Concrete relic classes: sealed ones in src/Relics/Sts1.</summary>
	private static IEnumerable<(string Id, string Code)> Relics() =>
		Directory.EnumerateFiles(Path.Combine(Repo, "src/Relics/Sts1"), "Sts1*.cs")
			.Select(f => (File.ReadAllText(f), Path.GetFileNameWithoutExtension(f)))
			.Where(x => Regex.IsMatch(x.Item1, @"public sealed class " + x.Item2 + @"\b"))
			.Select(x => (ModelIds.FromClassName(x.Item2), x.Item1));

	[Fact]
	public void TheNineteenRelics_AreThere() => Assert.Equal(19, Relics().Count());

	[Fact]
	public void Relics_HaveArtAndText()
	{
		List<string> problems = new();
		var tables = new[] { "eng", "fra" }.ToDictionary(l => l, Table);
		foreach (var (id, code) in Relics())
		{
			foreach (string suffix in new[] { "", "_outline", "_large" })
			{
				string art = Path.Combine(Repo, "godot/SlaySpoon/images/relics/sts1", id.ToLowerInvariant() + suffix + ".png");
				if (!File.Exists(art)) problems.Add("art: " + art);
			}
			// Vars the class or its base declares, by StS2's var names.
			HashSet<string> vars = new();
			string all = code + (code.Contains(": Sts1EnergyBossRelic") ? "new EnergyVar(" : "");
			foreach (Match m in Regex.Matches(all, @"new (Energy|Cards|Gold|MaxHp)Var\(")) vars.Add(m.Groups[1].Value);
			foreach (var (lang, table) in tables)
			{
				foreach (string key in new[] { "title", "description", "flavor" })
				{
					if (string.IsNullOrWhiteSpace(table.GetValueOrDefault($"{id}.{key}"))) problems.Add($"{lang} {id}.{key}");
				}
				foreach (Match m in Regex.Matches(table.GetValueOrDefault(id + ".description", ""), @"\{(\w+)[:}]"))
				{
					if (!vars.Contains(m.Groups[1].Value)) problems.Add($"{lang} {id}: {{{m.Groups[1].Value}}}");
				}
			}
		}
		Assert.Empty(problems);
	}

	[Theory]
	[InlineData(false, false, false, false)]
	[InlineData(false, true, true, false)]
	[InlineData(true, false, false, false)]
	[InlineData(true, true, false, true)]
	[InlineData(true, false, true, true)]
	public void Offered_OnlyInCustomRunsWithStS1Cards(bool custom, bool sts1Only, bool souvenir, bool expected) =>
		Assert.Equal(expected, Sts1RelicRule.Offered(custom, sts1Only, souvenir));

	[Theory]
	[InlineData(10, 15)]
	[InlineData(5, 8)] // 7.5 rounds away from zero
	[InlineData(1, 2)]
	[InlineData(0, 0)]
	public void MagicFlower_HealsHalfMore(int heal, int expected) => Assert.Equal(expected, Sts1RelicRule.CombatHeal(heal));

	[Theory]
	[InlineData(true, false, 0, true)]
	[InlineData(true, false, 1, false)]
	[InlineData(true, true, 0, false)]
	[InlineData(false, false, 0, false)]
	public void WristBlade_OnlyZeroCostAttacks(bool attack, bool costsX, int cost, bool expected) =>
		Assert.Equal(expected, Sts1RelicRule.WristBlade(attack, costsX, cost));

	[Fact]
	public void Inserter_TriggersEveryOtherTurn()
	{
		int counter = 0;
		List<bool> triggers = new();
		for (int turn = 0; turn < 4; turn++)
		{
			counter = Sts1RelicRule.InserterCounter(counter);
			triggers.Add(counter == 0);
		}
		Assert.Equal(new[] { false, true, false, true }, triggers);
	}

	[Theory]
	[InlineData(3, 2, 1)]
	[InlineData(5, 2, 3)]
	[InlineData(2, 2, 1)]
	[InlineData(1, 2, 1)]
	[InlineData(0, 2, 0)]
	public void BustedCrown_KeepsAtLeastOne(int offered, int removed, int expected) =>
		Assert.Equal(expected, Sts1RelicRule.BustedCrownOptions(offered, removed));

	[Fact]
	public void SacredBark_Doubles()
	{
		Assert.Equal(2, Sts1RelicRule.PotionFactor(true));
		Assert.Equal(1, Sts1RelicRule.PotionFactor(false));
	}
}
