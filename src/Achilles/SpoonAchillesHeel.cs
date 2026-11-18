using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Achilles;

/// <summary>
/// Achilles' Heel (owner, 2026-11-18): the holder dies to any unblocked powered attack damage (<see cref="SpoonHeelPower"/>),
/// gains Dexterity per act past the first, and deals 1 more damage per 5 Block. Art: a Pixabay photo (docs/backlog.md).
/// </summary>
public sealed class SpoonAchillesHeel : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_achilles_heel";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("BlockPerBonus", AchillesRule.BlockPerBonus) };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<SpoonHeelPower>(), HoverTipFactory.FromPower<DexterityPower>() };

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is not CombatRoom) return;
		Flash();
		var context = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<SpoonHeelPower>(context, Owner.Creature, 1, Owner.Creature, null);
		int dexterity = AchillesRule.DexterityFor(Owner.RunState.CurrentActIndex);
		if (dexterity > 0) await PowerCmd.Apply<DexterityPower>(context, Owner.Creature, dexterity, Owner.Creature, null);
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (dealer != Owner.Creature || !props.IsPoweredAttack()) return 0m;
		return AchillesRule.BlockBonus(Owner.Creature.Block);
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class AchillesHeelPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonAchillesHeel>()).ToList();
	}
}
