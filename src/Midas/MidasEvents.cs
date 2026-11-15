using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Events;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Midas;

/// <summary>King Midas rule 3: every "?" event is a gold event, and only its gold options remain.</summary>
public static class MidasEvents
{
	/// <summary>
	/// Draws the room's gold event on the mod's own Rng (run seed + floor): vanilla streams do not move and every co-op
	/// client draws the same one. A follow-up another model already chose (the Lantern Key's War Historian) stays.
	/// </summary>
	public static EventModel Draw(IRunState? runState, EventModel currentEvent)
	{
		if (runState is not RunState run || currentEvent is WarHistorianRepy) return currentEvent;
		List<EventModel> candidates = ModelDb.AllEvents.Cast<EventModel>().Concat(Sts1Event.All)
			.Where(e => MidasRule.GoldEvents.Contains(e.Id.Entry))
			.DistinctBy(e => e.Id)
			.Where(e => Eligible(e, run))
			.ToList();
		Rng rng = new(run.Rng.Seed, $"SlaySpoonMidas_{run.TotalFloor}");
		int index = MidasRule.Pick(candidates.Count, rng.NextInt(int.MaxValue));
		if (index < 0) return currentEvent;
		SpoonLog.Info($"King Midas: {candidates[index].Id.Entry} replaces {currentEvent.Id.Entry}");
		return candidates[index];
	}

	private static bool Eligible(EventModel e, RunState run) => e switch
	{
		// The StS1 events skip their card-set rule: their gold options need no StS1 card.
		Sts1MoaiHead => run.Players.All(p => p.Relics.Any(r => r is Sts1GoldenIdol)),
		Sts1Event => true,
		_ => e.IsAllowed(run),
	};
}

/// <summary>Drops the non-gold options of a gold event's page (<see cref="MidasRule.KeptOptions"/>).</summary>
[HarmonyPatch(typeof(EventModel), "SetEventState")]
public static class MidasEventOptionsPatch
{
	public static void Prefix(EventModel __instance, ref IEnumerable<EventOption> eventOptions)
	{
		if (__instance.Owner is not { } owner || !SpoonRoiMidas.IsActive(owner.RunState)) return;
		List<EventOption> options = eventOptions.ToList();
		List<int> kept = MidasRule.KeptOptions(options.Select(o => o.TextKey).ToList(), options.Select(o => !o.IsLocked).ToList()).ToList();
		eventOptions = kept.Count == options.Count ? options : kept.Select(i => options[i]).ToList();
	}
}
