using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Blood;

/// <summary>Blood's vampirism relic, given at the start: attacks you play heal 20 % of the unblocked damage they deal.</summary>
public sealed class SpoonBloodFang : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	/// <summary>The mod ships no relic art: the icons come from Blood-Soaked Rose.</summary>
	protected override string IconBaseName => "blood_soaked_rose";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Percent", BloodRule.LeechPercent) };

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
		Creature target, CardModel? cardSource)
	{
		if (dealer != Owner.Creature || cardSource is not { Type: CardType.Attack } || Owner.Creature.IsDead) return;
		int heal = BloodRule.Leech(result.UnblockedDamage);
		if (heal <= 0) return;
		Flash();
		await CreatureCmd.Heal(Owner.Creature, heal);
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class BloodFangPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonBloodFang>()).ToList();
	}
}
