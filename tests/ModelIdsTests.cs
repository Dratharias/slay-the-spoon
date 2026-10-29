using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class ModelIdsTests
{
	[Theory]
	[InlineData("Sts1HeavyBlade", "STS1_HEAVY_BLADE")]
	[InlineData("Sts1Cleave", "STS1_CLEAVE")]
	[InlineData("Sts1Jax", "STS1_JAX")]
	[InlineData("SpoonMonsterHp150", "SPOON_MONSTER_HP150")] // logged by step 1 in game
	[InlineData("SpoonCardSetSts1Only", "SPOON_CARD_SET_STS1_ONLY")]
	[InlineData("Sts1IroncladCardPool", "STS1_IRONCLAD_CARD_POOL")]
	public void ModelIds_FromClassName(string className, string expected)
	{
		Assert.Equal(expected, ModelIds.FromClassName(className));
	}
}
