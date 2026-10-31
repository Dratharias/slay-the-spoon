namespace SlaySpoon.Core;

/// <summary>
/// Boss rewards on act 3 once The Ending is appended. Vanilla gives no rewards to either boss of a last act (it
/// checks only "is this the last act"); with the Ending behind it act 3 is not last, so both would pay out.
/// Owner decision (2026-10-30): the first of the two A10 bosses behaves as in vanilla's last act, the second pays.
/// </summary>
public static class EndingBossRewardRule
{
	/// <summary>True when the boss room's reward generation must be skipped, as vanilla does for a last-act boss.</summary>
	public static bool SuppressRewards(bool isBossRoom, bool onLastAct, bool nextActIsEnding, bool hasSecondBoss, bool atFirstBoss) =>
		isBossRoom && !onLastAct && nextActIsEnding && hasSecondBoss && atFirstBoss;
}
