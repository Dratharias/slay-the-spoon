using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Events;

/// <summary>
/// Turns an act 2 event room into a StS1 event, after the vanilla hooks. The roll draws from its own Rng, seeded from
/// the run seed, act and visited event count: no vanilla stream moves, and every co-op client picks the same event.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyNextEvent))]
public static class EventInjectionPatch
{
	public static void Postfix(IRunState runState, EventModel currentEvent, ref EventModel __result)
	{
		// Another hook (Lantern Key) already chose this room's event.
		if (!ReferenceEquals(__result, currentEvent) || runState is not RunState run || run.CurrentActIndex != Sts1EventRule.ActIndex)
		{
			return;
		}
		List<Sts1Event> candidates = Sts1Event.All
			.Where(e => !run.VisitedEventIds.Contains(e.Id) && e.IsAllowed(run))
			.ToList();
		if (candidates.Count == 0)
		{
			return;
		}
		Rng rng = new(run.Rng.Seed, $"SlaySpoonSts1Event{run.CurrentActIndex}_{run.VisitedEventIds.Count}");
		if (!Sts1EventRule.ShouldReplace(run.CurrentActIndex, candidates.Count, rng.NextFloat()))
		{
			return;
		}
		__result = candidates[rng.NextInt(candidates.Count)];
		SpoonLog.Info($"StS1 event: {__result.Id.Entry} replaces {currentEvent.Id.Entry}");
	}
}
