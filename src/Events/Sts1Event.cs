using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.CardSets;
using SlaySpoon.Core;
using SlaySpoon.Relics;

namespace SlaySpoon.Events;

/// <summary>
/// A StS1 event that hands out an event-only StS1 card (step 3d plan) or a StS1 special relic (backlog wave). Vanilla acts never list it: the injection in
/// <see cref="EventInjectionPatch"/> puts it in an act 2 event room when <see cref="Sts1EventRule"/> allows it.
/// </summary>
public abstract class Sts1Event : EventModel
{
	public abstract Sts1EventKind Kind { get; }

	/// <summary>The StS1 events, canonical, in <see cref="Sts1EventKind"/> order.</summary>
	public static IReadOnlyList<Sts1Event> All => new Sts1Event[]
	{
		ModelDb.Event<Sts1Vampires>(),
		ModelDb.Event<Sts1Augmenter>(),
		ModelDb.Event<Sts1Nest>(),
		ModelDb.Event<Sts1GoldenIdolEvent>(),
		ModelDb.Event<Sts1ForgottenAltar>(),
		ModelDb.Event<Sts1MoaiHead>(),
	};

	/// <summary>StS1's art, converted by <c>tools/sts1/art.py --events</c> (card events) or <c>tools/sts1/event_art.py</c>.</summary>
	public string PortraitPath => $"res://SlaySpoon/images/events/{Id.Entry.ToLowerInvariant()}.png";

	public override bool IsAllowed(IRunState runState)
	{
		return Sts1EventRule.IsAllowed(Kind, IsSts1Run(runState), runState.Players.Count,
			runState.Players.Count(OwnsSouvenir));
	}

	/// <summary>The owner may take the StS1 card; otherwise the option shows locked.</summary>
	protected bool CardUnlocked => Owner != null && Sts1EventRule.CardUnlocked(IsSts1Run(Owner.RunState), OwnsSouvenir(Owner));

	private static bool IsSts1Run(IRunState run)
	{
		return CardSetPools.ModeOf(run) == CardSetMode.Sts1Only;
	}

	private static bool OwnsSouvenir(Player player)
	{
		return player.Relics.Any(r => r is SpoonSpireSouvenir);
	}
}

/// <summary>Vanilla looks for event portraits under <c>res://images/events/</c>; the StS1 events ship their own.</summary>
[HarmonyPatch(typeof(EventModel), "InitialPortraitPath", MethodType.Getter)]
public static class Sts1EventPortraitPatch
{
	public static void Postfix(EventModel __instance, ref string __result)
	{
		if (__instance is Sts1Event sts1)
		{
			__result = sts1.PortraitPath;
		}
	}
}
