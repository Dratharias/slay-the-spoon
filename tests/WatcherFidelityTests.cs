using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class WatcherFidelityTests
{
    [Theory]
    [InlineData(true, true, false, false, true)] // Generated Smite
    [InlineData(true, true, true, false, false)] // Upgradable Status/Curse
    [InlineData(true, true, false, true, false)] // Foreign Influence pick
    [InlineData(false, true, false, false, false)] // Another player's creation
    [InlineData(true, false, false, false, false)] // Already upgraded
    public void MasterRealityCreationSources(bool holder, bool upgradable, bool status, bool foreign, bool expected) =>
        Assert.Equal(expected, WatcherFidelityRule.MasterReality(holder, upgradable, status, foreign));

    [Theory]
    [InlineData(3, 9, 12)] // Wrath, ordinary Halt
    [InlineData(5, 11, 16)] // +2 Dexterity applies to both components
    [InlineData(4, 14, 18)] // Upgraded
    [InlineData(0, 5, 5)] // Negative Dexterity independently clamps the first component
    [InlineData(3, 0, 3)] // Outside Wrath
    public void HaltCombinesModifiedAmountsIntoOneGain(int normal, int extra, int expected) =>
        Assert.Equal(expected, WatcherFidelityRule.HaltBlock(normal, extra));
}
