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

	[Fact]
	public void LimitFor_VanillaCharacterKeepsItsOwnMax()
	{
		var stats = new[] { ("IRONCLAD", 7), ("STS1_WATCHER", 0), ("SILENT", 3) };
		Assert.Equal(3, WatcherAscension.LimitFor("SILENT", 3, "STS1_WATCHER", stats));
	}

	[Fact]
	public void LimitFor_WatcherTakesTheBestOtherCharacter()
	{
		var stats = new[] { ("IRONCLAD", 7), ("STS1_WATCHER", 12), ("SILENT", 3) };
		Assert.Equal(7, WatcherAscension.LimitFor("STS1_WATCHER", 12, "STS1_WATCHER", stats));
	}

	[Fact]
	public void LimitFor_WatcherAloneIsZero()
	{
		Assert.Equal(0, WatcherAscension.LimitFor("STS1_WATCHER", 5, "STS1_WATCHER", new[] { ("STS1_WATCHER", 5) }));
	}
}
