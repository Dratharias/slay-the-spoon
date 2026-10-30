using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherAvailabilityTests
{
	private const string Watcher = "STS1_WATCHER";

	[Fact]
	public void IsAvailable_WithSts1Only()
	{
		Assert.True(WatcherAvailability.IsAvailable(new[] { "SPOON_MONSTER_HP150", "SPOON_CARD_SET_STS1_ONLY" }));
	}

	[Fact]
	public void IsAvailable_WithSouvenir()
	{
		Assert.True(WatcherAvailability.IsAvailable(new[] { "SPOON_SPIRE_SOUVENIR_MODIFIER" }));
	}

	[Fact]
	public void IsAvailable_FalseWithMonsterHpOnly()
	{
		Assert.False(WatcherAvailability.IsAvailable(new[] { "SPOON_MONSTER_HP150" }));
		Assert.False(WatcherAvailability.IsAvailable(Array.Empty<string>()));
	}

	[Fact]
	public void Fallback_ReturnsPreviousWhenUnavailable()
	{
		Assert.Equal("SILENT", WatcherAvailability.Fallback(Watcher, "SILENT", available: false));
		// A vanilla pick never moves.
		Assert.Equal("DEFECT", WatcherAvailability.Fallback("DEFECT", "SILENT", available: false));
	}

	[Fact]
	public void Fallback_KeepsWatcherWhenAvailable()
	{
		Assert.Equal(Watcher, WatcherAvailability.Fallback(Watcher, "SILENT", available: true));
	}
}
