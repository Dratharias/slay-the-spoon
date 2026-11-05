using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class CampfireGiftRuleTests
{
	[Theory]
	[InlineData(false, true, 50, true)]
	[InlineData(false, true, 49, false)]
	[InlineData(false, false, 999, false)]
	[InlineData(true, true, 999, false)]
	public void ReceiverMustBeAnAllyAbleToPay(bool isGiver, bool alive, int gold, bool expected) =>
		Assert.Equal(expected, CampfireGiftRule.CanReceive(isGiver, alive, gold));

	[Theory]
	[InlineData("Attack", true, true)]
	[InlineData("Curse", true, false)]
	[InlineData("Skill", false, false)]
	public void CursesAndUnremovableCardsStay(string type, bool removable, bool expected) =>
		Assert.Equal(expected, CampfireGiftRule.CanGift(type, removable));
}
