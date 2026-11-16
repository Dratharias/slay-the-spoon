using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class SpoonTestStartRuleTests
{
	private const string Exe = "/games/Slay the Spire 2/SlayTheSpire2";

	[Theory]
	[InlineData("/games/Slay the Spire 2/mods/SlaySpoon")]
	[InlineData("/games/Slay the Spire 2/mods/SlaySpoon/")]
	[InlineData("/games/Slay the Spire 2/mods/dev/SlaySpoon")]
	public void LocalModsFolderShowsTheButton(string modPath) =>
		Assert.True(SpoonTestStartRule.IsLocalInstall("ModsDirectory", modPath, Exe));

	[Theory]
	[InlineData("SteamWorkshop", "/steam/workshop/content/2868840/3829630403")]
	[InlineData("SteamWorkshop", "/games/Slay the Spire 2/mods_STEAMTEST/SlaySpoon")]
	[InlineData("SteamWorkshop", "/games/Slay the Spire 2/mods/SlaySpoon")]
	[InlineData("None", "/games/Slay the Spire 2/mods/SlaySpoon")]
	[InlineData(null, "/games/Slay the Spire 2/mods/SlaySpoon")]
	[InlineData("ModsDirectory", null)]
	[InlineData("ModsDirectory", "")]
	[InlineData("ModsDirectory", "/games/Slay the Spire 2/mods")]
	[InlineData("ModsDirectory", "/games/Slay the Spire 2/mods_STEAMTEST/SlaySpoon")]
	[InlineData("ModsDirectory", "/games/Slay the Spire 2/modsX/SlaySpoon")]
	[InlineData("ModsDirectory", "/elsewhere/mods/SlaySpoon")]
	[InlineData("ModsDirectory", "/games/Slay the Spire 2/mods/../other/SlaySpoon")]
	public void AnythingElseHidesIt(string? source, string? modPath) =>
		Assert.False(SpoonTestStartRule.IsLocalInstall(source, modPath, Exe));

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void UnknownExecutableHidesIt(string? exe) =>
		Assert.False(SpoonTestStartRule.IsLocalInstall("ModsDirectory", "/games/Slay the Spire 2/mods/SlaySpoon", exe));

	[Fact]
	public void WindowsSeparatorsAreAccepted() =>
		Assert.True(SpoonTestStartRule.IsLocalInstall("ModsDirectory", @"C:\Games\StS2\mods\SlaySpoon", @"C:\Games\StS2\SlayTheSpire2.exe"));
}
