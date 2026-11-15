using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// Spoon mode: the Max HP event and Ancient relics join the ordinary grab bag deques. The bag pulls by the deque key (and
/// <c>IsAllowed</c>), never by the relic's own rarity, so no pull filter needs patching.
/// </summary>
internal static class SpoonRelics
{
	private static readonly AccessTools.FieldRef<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>> Deques =
		AccessTools.FieldRefAccess<RelicGrabBag, Dictionary<RelicRarity, List<RelicModel>>>("_deques");

	public static void AddTo(RelicGrabBag bag, Rng rng)
	{
		var deques = Deques(bag);
		foreach (var (entry, dequeName) in SpoonRule.ExtraRelics)
		{
			RelicModel? relic = ModelDb.AllRelics.FirstOrDefault(r => r.Id.Entry == entry);
			if (relic == null || !Enum.TryParse(dequeName, out RelicRarity rarity) || bag.Contains(relic)) continue;
			if (!deques.TryGetValue(rarity, out var list)) deques[rarity] = list = [];
			list.Insert(rng.NextInt(list.Count + 1), relic);
		}
	}
}
