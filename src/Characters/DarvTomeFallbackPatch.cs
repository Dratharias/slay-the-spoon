using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using SlaySpoon.Core;

namespace SlaySpoon.Characters;

/// <summary>
/// Darv offers a Dusty Tome half the time; <c>DustyTomeBypass</c> leaves it without a card when the owner's pool has no
/// Ancient card (the Watcher's), and vanilla would then crash when the relic is shown or taken. The Tome option becomes
/// one more Darv relic the owner may take, drawn on the event's own Rng (the same on every peer).
/// </summary>
[HarmonyPatch(typeof(Darv), "GenerateInitialOptions")]
public static class DarvTomeFallbackPatch
{
	private static readonly AccessTools.FieldRef<IList> ValidRelicSets =
		AccessTools.StaticFieldRefAccess<IList>(AccessTools.Field(typeof(Darv), "_validRelicSets"));

	private static readonly Func<AncientEventModel, RelicModel, string, string?, EventOption> RelicOption =
		AccessTools.MethodDelegate<Func<AncientEventModel, RelicModel, string, string?, EventOption>>(
			AccessTools.Method(typeof(AncientEventModel), "RelicOption", [typeof(RelicModel), typeof(string), typeof(string)]));

	public static void Postfix(Darv __instance, ref IReadOnlyList<EventOption> __result)
	{
		int tome = __result.ToList().FindIndex(o => o.Relic is DustyTome { AncientCard: null });
		if (tome < 0 || __instance.Owner is not { } owner)
			return;
		List<RelicModel> candidates = Candidates(owner);
		int pick = DarvTomeRule.Replacement(candidates.Select(r => r.Id.Entry).ToList(),
			__result.Where(o => o.Relic != null).Select(o => o.Relic!.Id.Entry).ToList(), __instance.Rng.NextInt(int.MaxValue));
		List<EventOption> options = __result.ToList();
		if (pick < 0)
			options.RemoveAt(tome);
		else
			options[tome] = RelicOption(__instance, candidates[pick].ToMutable(), "INITIAL", null);
		SpoonLog.Info($"Darv: the Dusty Tome for {owner.Character.Id.Entry} is replaced by {(pick < 0 ? "nothing" : candidates[pick].Id.Entry)}");
		__result = options;
	}

	/// <summary>Darv's relics whose set accepts the owner, in Darv's order.</summary>
	private static List<RelicModel> Candidates(Player owner)
	{
		var list = new List<RelicModel>();
		foreach (object set in ValidRelicSets())
		{
			var t = Traverse.Create(set);
			if (t.Field("filter").GetValue<Func<Player, bool>>()(owner))
				list.AddRange(t.Field("relics").GetValue<RelicModel[]>());
		}
		return list;
	}
}
