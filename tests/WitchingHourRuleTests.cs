using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WitchingHourRuleTests
{
	[Theory]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, true)]
	[InlineData(10, false)]
	public void UnplayableOnOddTurns(int round, bool blocked) => Assert.Equal(blocked, WitchingHourRule.BlocksPlay(round));

	[Fact]
	public void OtherCardsCostOneMoreOnOddTurnsInHand()
	{
		Assert.Equal(2m, WitchingHourRule.CostWith(1m, 1, true));
		Assert.Equal(1m, WitchingHourRule.CostWith(0m, 3, true));
		Assert.Equal(1m, WitchingHourRule.CostWith(1m, 2, true));
		Assert.Equal(1m, WitchingHourRule.CostWith(1m, 1, false));
		Assert.Equal(-1m, WitchingHourRule.CostWith(-1m, 1, true));
	}

	[Fact]
	public void PulledEightyPercentOnLaterOddTurns()
	{
		Assert.True(WitchingHourRule.Pulls(3, 0));
		Assert.True(WitchingHourRule.Pulls(3, 79));
		Assert.False(WitchingHourRule.Pulls(3, 80));
		Assert.False(WitchingHourRule.Pulls(2, 0));
		Assert.False(WitchingHourRule.Pulls(1, 0));
	}

	[Fact]
	public void GrimoirePlayStunsOneTarget()
	{
		var row = new CurseRow(WitchingHourRule.Id, "Stun", 0, 0, null, null, 0);
		Assert.Equal(1, CurseRule.PlayedAmount(row, 0, 5, 5));
		Assert.True(CurseRule.NeedsTarget(row));
	}

	private static readonly string Repo = FindRepo();

	private static string FindRepo()
	{
		string? dir = AppContext.BaseDirectory;
		while (dir != null && !File.Exists(Path.Combine(dir, "Makefile"))) dir = Path.GetDirectoryName(dir);
		return dir ?? throw new InvalidOperationException("repo root not found");
	}

	[Fact]
	public void HasTextInBothLanguagesAndPortrait()
	{
		foreach (string lang in new[] { "eng", "fra" })
		{
			string text = File.ReadAllText(Path.Combine(Repo, $"godot/SlaySpoon/localization/{lang}/cards.json"));
			foreach (string key in new[] { "title", "description", "cantPlay" })
				Assert.Contains($"\"{WitchingHourRule.Id}.{key}\"", text);
		}
		Assert.True(File.Exists(Path.Combine(Repo, "godot/SlaySpoon/images/cards/curses/spoon_witching_hour.png")));
		Assert.Contains($"\"{WitchingHourRule.Id}\"", File.ReadAllText(Path.Combine(Repo, "data/curses/curses.json")));
	}

	[Fact]
	public void StreamName() => Assert.Equal("witching_hour_12_3_4", WitchingHourRule.PullStream(12, 3, 4));

	[Fact]
	public void ForgedCostPenaltyCapsAtTwo()
	{
		Assert.Equal(new[] { 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 }, Enumerable.Range(0, 10).Select(WitchingHourRule.CostPenaltyAt));
		Assert.Equal(4m, WitchingHourRule.CostWith(2, 3, curseInHand: true, level: 9));
		Assert.Equal(2m, WitchingHourRule.CostWith(2, 4, curseInHand: true, level: 9));
	}

	[Fact]
	public void ForgedHpLossBeatsBadLuck()
	{
		Assert.Equal(new[] { 0, 15, 24, 33, 42, 51, 60, 69, 78, 87 },
			Enumerable.Range(0, 10).Select(lv => WitchingHourRule.HpLossAt(lv, forged: true)));
		Assert.Equal(0, WitchingHourRule.HpLossAt(1, forged: false));
		Assert.True(WitchingHourRule.HpLossAt(9, forged: true) > 76);
	}

	[Fact]
	public void ForgedStunLastsTwoTurnsFromPlusFive()
	{
		Assert.Equal(new[] { 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 }, Enumerable.Range(0, 10).Select(WitchingHourRule.StunTurnsAt));
		var row = new CurseRow(WitchingHourRule.Id, "Stun", 0, 0, null, null, 0);
		Assert.Equal(2, CurseRule.PlayedAmount(row, 9, 0, 0));
		Assert.Equal(1, CurseRule.PlayedAmount(row, true, 0, 0));
	}

	[Fact]
	public void ForgedDefectsSkipOddEvenAndEcho()
	{
		for (int seed = 0; seed < 50; seed++)
		{
			var rng = new Random(seed);
			var set = ForgedCurseRule.Merge(CurseDefect.None, CurseDefect.Even | CurseDefect.Echo, 9, n => rng.Next(n),
				WitchingHourRule.ExcludedDefects);
			Assert.Equal(CurseDefect.None, set & WitchingHourRule.ExcludedDefects);
			Assert.Equal(3, ForgedCurseRule.All.Count(d => set.HasFlag(d)));
		}
	}
}
