using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ModifierListTests
{
	[Fact] public void CustomRunOption_ListsAct4() => Assert.Contains("SpoonAct4", CustomRunOptions.TypeNames);
	[Fact] public void CustomRunOption_IsNotOfferedInDaily() => Assert.False(CustomRunOptions.OfferedIn(gameMode: "Daily"));
	[Fact] public void CustomRunOption_IsNotOfferedInStandard() => Assert.False(CustomRunOptions.OfferedIn(gameMode: "Standard"));
	[Fact] public void CustomRunOption_IsOfferedInCustom() => Assert.True(CustomRunOptions.OfferedIn(gameMode: "Custom"));
}
