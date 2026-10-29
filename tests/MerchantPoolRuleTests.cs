using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MerchantPoolRuleTests
{
	[Fact]
	public void Vanilla_and_sts1_cards_of_the_character_are_its_offer()
	{
		Assert.True(MerchantPoolRule.IsCharacterOffer(new[] { "IRONCLAD", "STS1_IRONCLAD", "IRONCLAD" }, "IRONCLAD", "STS1_IRONCLAD"));
	}

	[Fact]
	public void Sts1_only_offer_is_the_characters()
	{
		Assert.True(MerchantPoolRule.IsCharacterOffer(new[] { "STS1_IRONCLAD", "STS1_IRONCLAD" }, "IRONCLAD", "STS1_IRONCLAD"));
	}

	[Fact]
	public void Colorless_offer_is_not_the_characters()
	{
		Assert.False(MerchantPoolRule.IsCharacterOffer(new[] { "COLORLESS", "STS1_COLORLESS" }, "IRONCLAD", "STS1_IRONCLAD"));
		Assert.False(MerchantPoolRule.IsCharacterOffer(new[] { "STS1_IRONCLAD", "COLORLESS" }, "IRONCLAD", "STS1_IRONCLAD"));
	}

	[Fact]
	public void Character_without_sts1_pool_accepts_only_its_own()
	{
		Assert.True(MerchantPoolRule.IsCharacterOffer(new[] { "NECROBINDER" }, "NECROBINDER", null));
		Assert.False(MerchantPoolRule.IsCharacterOffer(new[] { "NECROBINDER", "STS1_IRONCLAD" }, "NECROBINDER", null));
	}
}
