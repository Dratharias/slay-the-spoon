using SlaySpoon.Core;
using Xunit;

namespace SlaySpoon.Tests;

public class MonsterDamageRuleTests
{
	[Fact]
	public void BeatOfDeathIsScaledOnceAsAnEnemyEffect()
	{
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: false, enemyStatus: false, enemyEffect: true));
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: true, targetIsEnemy: false, enemyStatus: false, enemyEffect: false));
	}

	[Fact]
	public void Enemy_dealer_always_scales()
	{
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: true, targetIsEnemy: false, enemyStatus: false, enemyEffect: false));
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: true, targetIsEnemy: true, enemyStatus: false, enemyEffect: false));
	}

	[Fact]
	public void Status_card_from_an_enemy_scales_on_the_player()
	{
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: false, enemyStatus: true, enemyEffect: false));
	}

	[Fact]
	public void Enemy_power_effect_scales_on_the_player()
	{
		Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: false, enemyStatus: false, enemyEffect: true));
	}

	[Fact]
	public void Player_damage_to_enemies_never_scales()
	{
		Assert.False(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: true, enemyStatus: true, enemyEffect: true));
	}

	[Fact]
	public void Other_damage_to_the_player_stays_vanilla()
	{
		Assert.False(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: false, enemyStatus: false, enemyEffect: false));
	}
}
