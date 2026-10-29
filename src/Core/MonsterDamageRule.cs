namespace SlaySpoon.Core;

/// <summary>Which damage the Monster Damage percent scales: everything an enemy causes, never the players' own damage.</summary>
public static class MonsterDamageRule
{
	/// <param name="dealerIsEnemy">An enemy deals the hit (attacks, thorns-like enemy powers).</param>
	/// <param name="targetIsEnemy">The hit lands on an enemy.</param>
	/// <param name="enemyStatus">A Status card an enemy put in the player's piles deals it (Burn, Toxic, Infection…).</param>
	/// <param name="enemyEffect">An enemy's lingering effect deals it with no dealer (Constrict, Galvanic).</param>
	public static bool Scales(bool dealerIsEnemy, bool targetIsEnemy, bool enemyStatus, bool enemyEffect)
	{
		if (dealerIsEnemy)
		{
			return true;
		}
		return !targetIsEnemy && (enemyStatus || enemyEffect);
	}
}
