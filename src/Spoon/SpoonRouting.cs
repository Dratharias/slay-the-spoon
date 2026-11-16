using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Act4;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// Inserts the Spoon act after act 3 (before The Ending when present) when the last act 3 boss dies and a player has
/// 300 max HP or more (spec rule 4). From then on vanilla reads the act count: the act 3 boss is an ordinary boss
/// (rewards, no win time, its reward screen enters the Spoon act); the Spoon is the final boss when it is last, or an
/// ordinary boss in front of The Ending (spec rule 7).
/// </summary>
/// <remarks>
/// It runs in <c>AfterCombatVictory</c>, a hook every co-op peer runs at the same point of the synced combat end, on
/// state every peer shares (act index, map coord, max HPs in player order), and before vanilla sets the win time and
/// saves the run. The act list is in the save, so a reload keeps it (<c>ActModel.FromSave</c> finds the act by id).
/// </remarks>
public static class SpoonRouting
{
	/// <summary>Inserts the Spoon act when <see cref="SpoonRule.ShouldInsert"/> allows it; true when it inserted.</summary>
	public static bool EnsureSpoonAct(RunState state, CombatRoom room)
	{
		var map = state.Map;
		var bossCoord = map.SecondBossMapPoint?.coord ?? map.BossMapPoint.coord;
		bool atLastBoss = room.RoomType == RoomType.Boss && state.CurrentMapCoord == bossCoord;
		var maxHps = state.Players.Select(p => p.Creature.MaxHp).ToList();
		bool alreadyInserted = state.Acts.Any(a => a is SpoonAct);
		if (!SpoonRule.ShouldInsert(SpoonSpoon.IsActive(state), state.CurrentActIndex == VanillaActIndex.Last, atLastBoss,
			    maxHps, alreadyInserted))
			return false;

		bool endingPresent = state.Acts.Count == 4 && state.Acts[3] is Sts1EndingAct;
		int index = SpoonRule.InsertIndex(state.Acts.Count, endingPresent);
		if (index < 0)
		{
			SpoonLog.Error($"spoon: unexpected act list ({string.Join(", ", state.Acts.Select(a => a.Id.Entry))}); no Spoon act");
			return false;
		}
		// A new list, as vanilla's debug act switch does: the old list may be enumerated elsewhere.
		var acts = new List<ActModel>(state.Acts);
		acts.Insert(index, SpoonAct.CreateForRun(state));
		Act4Routing.SetActs(state, acts);
		SpoonLog.Info($"spoon: Spoon act inserted at {index}");
		// The act 3 boss is no longer the final boss: a recorded WinTime would freeze the run clock.
		RunManager manager = RunManager.Instance;
		if (EndingRouteRule.ShouldResetWinTime(true, manager.WinTime))
		{
			SpoonLog.Info($"spoon: stale WinTime {manager.WinTime} cleared");
			manager.WinTime = 0;
		}
		return true;
	}
}
