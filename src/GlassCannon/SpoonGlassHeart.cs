using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.GlassCannon;

/// <summary>
/// Glass Heart (Glass Cannon, owner 2026-11-19): at the start of every combat the holder gains 99 Frail and Frenzy
/// (<see cref="SpoonFrenzyPower"/>). Art: a placeholder (docs/backlog.md).
/// </summary>
public sealed class SpoonGlassHeart : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_glass_heart";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<SpoonFrenzyPower>(), HoverTipFactory.FromPower<FrailPower>() };

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is not CombatRoom) return;
		Flash();
		var context = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<FrailPower>(context, Owner.Creature, GlassCannonRule.FrailAmount, Owner.Creature, null);
		await PowerCmd.Apply<SpoonFrenzyPower>(context, Owner.Creature, 1, Owner.Creature, null);
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class GlassHeartPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonGlassHeart>()).ToList();
	}
}
