using SlaySpoon.Core;
using Xunit;
namespace SlaySpoon.Tests;
public class WatcherOrobasTests
{
    [Theory]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public void OnlyWatcherWithoutToothGetsFallback(bool watcher, bool tooth, bool expected) =>
        Assert.Equal(expected, WatcherOrobasRule.UseFallback(watcher, tooth));

    [Fact]
    public void BothEventRngOutcomesAreDistinctRelics()
    {
        Assert.True(WatcherOrobasRule.HolyWater(0));
        Assert.False(WatcherOrobasRule.HolyWater(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WatcherOrobasRule.HolyWater(2));
    }
}
