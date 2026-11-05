using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace SlaySpoon.Relics.Events;

/// <summary>
/// A StS1 special relic handed out only by its StS1 event. Its art comes from StS1 (<c>tools/sts1/event_art.py</c>).
/// </summary>
public abstract class Sts1EventRelic : RelicModel
{
	private string Art => $"res://SlaySpoon/images/relics/events/{Id.Entry.ToLowerInvariant()}";

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool IsAllowedInShops => false;

	public override string PackedIconPath => Art + ".png";

	protected override string PackedIconOutlinePath => Art + "_outline.png";

	protected override string BigIconPath => Art + "_large.png";

	/// <summary>Every event relic the mod ships, for the pool below.</summary>
	public static IEnumerable<RelicModel> All => new RelicModel[]
	{
		ModelDb.Relic<Sts1GoldenIdol>(),
		ModelDb.Relic<Sts1BloodyIdol>(),
	};
}

/// <summary>Every relic needs a pool (RelicModel.Pool); the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class Sts1EventRelicPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Concat(Sts1EventRelic.All).ToList();
	}
}
