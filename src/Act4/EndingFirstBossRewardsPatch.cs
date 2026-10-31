using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>
/// A10 with The Ending appended: vanilla's reward generation skips a boss only when its act is the last, which no
/// longer holds for act 3, so both act 3 bosses would pay. The first one is skipped as vanilla does for a two-boss
/// last act (it returns before drawing from any Rng, so seeds stay vanilla); the second boss pays and its reward
/// screen enters act 4 as usual.
/// </summary>
[HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom))]
internal static class EndingFirstBossRewardsPatch
{
	private static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
	{
		try
		{
			var run = __instance.Player.RunState;
			ActMap map = run.Map;
			bool nextIsEnding = run.CurrentActIndex + 1 < run.Acts.Count && run.Acts[run.CurrentActIndex + 1] is Sts1EndingAct;
			bool atFirst = map.SecondBossMapPoint != null && run.CurrentMapCoord == map.BossMapPoint.coord;
			if (!EndingBossRewardRule.SuppressRewards(room.RoomType == MegaCrit.Sts2.Core.Rooms.RoomType.Boss,
				    run.CurrentActIndex >= run.Acts.Count - 1, nextIsEnding, map.SecondBossMapPoint != null, atFirst))
				return true;
			// What vanilla's own early return does: Room set, nothing generated.
			AccessTools.PropertySetter(typeof(RewardsSet), nameof(RewardsSet.Room)).Invoke(__instance, [room]);
			__result = __instance;
			SpoonLog.Info("act4: first A10 act 3 boss gives no rewards");
			return false;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"act4: boss rewards rule failed, vanilla rewards kept: {e.Message}");
			return true;
		}
	}
}
