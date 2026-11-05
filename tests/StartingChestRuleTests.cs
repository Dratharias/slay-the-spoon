using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class StartingChestRuleTests
{
	private static string? FirstUnseen(string[] pool, IReadOnlyCollection<string> shown) =>
		pool.FirstOrDefault(r => !shown.Contains(r));

	[Fact]
	public void DrawsTenDistinctRelics()
	{
		string[] pool = Enumerable.Range(0, 30).Select(i => $"r{i}").ToArray();
		var drawn = StartingChestRule.Draw<string>(StartingChestRule.Size, shown => FirstUnseen(pool, shown));
		Assert.Equal(10, drawn.Count);
		Assert.Equal(10, drawn.Distinct().Count());
	}

	[Fact]
	public void StopsWhenThePoolRunsOut()
	{
		string[] pool = { "a", "b", "c" };
		Assert.Equal(pool, StartingChestRule.Draw<string>(StartingChestRule.Size, shown => FirstUnseen(pool, shown)));
	}

	[Fact]
	public void RejectsARepeatedItem() =>
		Assert.Throws<InvalidOperationException>(() => StartingChestRule.Draw<string>(3, _ => "same"));
}
