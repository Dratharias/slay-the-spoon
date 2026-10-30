using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherProgressTests
{
	private static readonly string[] Vanilla = { "IRONCLAD", "SILENT", "REGENT", "NECROBINDER", "DEFECT" };

	[Fact]
	public void VanillaStatsCount_IgnoresWatcher()
	{
		Assert.Equal(5, WatcherProgress.VanillaStatsCount(Vanilla.Append("STS1_WATCHER"), Vanilla));
		Assert.Equal(4, WatcherProgress.VanillaStatsCount(Vanilla.Take(4).Append("STS1_WATCHER"), Vanilla));
	}

	[Fact]
	public void VanillaStatsCount_IgnoresRandomCharacterEntry()
	{
		Assert.Equal(1, WatcherProgress.VanillaStatsCount(new[] { "RANDOM_CHARACTER", "IRONCLAD" }, Vanilla));
	}

	[Fact]
	public void Cap_IsHighestVanilla()
	{
		Assert.Equal(7, WatcherAscension.Cap(new[] { 3, 7, 0, 5 }));
	}

	[Fact]
	public void Cap_ZeroWhenNoStats()
	{
		Assert.Equal(0, WatcherAscension.Cap(Array.Empty<int>()));
	}
}
