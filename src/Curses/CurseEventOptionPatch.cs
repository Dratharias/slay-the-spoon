using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Cursed Deck: the first page of every event (Ancients excepted) gets "Embrace the curse". It adds a random curse,
/// then one reward from <see cref="CurseRule.EventReward"/>, then ends the event as a vanilla "leave" does.
/// </summary>
[HarmonyPatch(typeof(EventModel), "SetEventState")]
[HarmonyPriority(Priority.Last)]
public static class CurseEventOptionPatch
{
	private const string Key = "SPOON_CURSED_DECK_EMBRACE";

	private static readonly Action<EventModel, LocString> SetFinished =
		AccessTools.MethodDelegate<Action<EventModel, LocString>>(AccessTools.Method(typeof(EventModel), "SetEventFinished"));

	/// <summary>Events whose first page was already seen.</summary>
	private static readonly ConditionalWeakTable<EventModel, object> Seen = new();

	public static void Prefix(EventModel __instance, ref IEnumerable<EventOption> eventOptions)
	{
		if (__instance.Owner is not { } owner || __instance is AncientEventModel || !SpoonCursedDeck.IsActive(owner.RunState)) return;
		if (Seen.TryGetValue(__instance, out _)) return;
		Seen.Add(__instance, new object());
		List<EventOption> options = eventOptions.ToList();
		if (options.Count == 0) return;
		var embrace = new EventOption(__instance, () => Embrace(__instance, owner), Key);
		options.Insert(CurseRule.EmbraceIndex(options.Select(o => o.TextKey).ToList()), embrace);
		eventOptions = options;
	}

	private static async Task Embrace(EventModel model, Player player)
	{
		IRunState run = player.RunState;
		Rng rng = new(run.Rng.Seed, CurseRule.EventStream(run.TotalFloor, run.GetPlayerSlotIndex(player)));
		await CursedDeckCurses.AddRandom(player, rng);

		HashSet<string> held = player.Relics.Select(r => r.Id.Entry).ToHashSet();
		List<RelicModel> left = CursedRelics.Pool.Where(r => !held.Contains(r.Id.Entry)).ToList();
		int roll = rng.NextInt(int.MaxValue);
		CurseEventReward reward = CurseRule.EventReward(roll, left.Count > 0);
		if (reward == CurseEventReward.Gold && SpoonBlood.IsActive(run)) reward = CurseEventReward.Heal;
		switch (reward)
		{
			case CurseEventReward.CursedRelic:
				string? id = CurseRule.PickCursedRelic(left.Select(r => r.Id.Entry).ToList(), new HashSet<string>(), rng.NextInt(int.MaxValue));
				await RelicCmd.Obtain(left.First(r => r.Id.Entry == id).ToMutable(), player);
				break;
			case CurseEventReward.Gold:
				await PlayerCmd.GainGold(CurseRule.EventGold, player);
				break;
			default:
				await CreatureCmd.Heal(player.Creature, CurseRule.EventHeal(player.Creature.MaxHp));
				break;
		}
		SetFinished(model, new LocString("events", Key + ".pages.DONE.description"));
	}
}
