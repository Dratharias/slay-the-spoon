using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>
/// Unholy Grimoire (owner, 2026-11-18), the first relic of the future curse deck mode: whenever one of the holder's
/// curses is exhausted, every enemy takes 5 damage (<see cref="CurseRule.GrimoireDamage"/>). Art: a Pixabay
/// illustration (sources in docs/backlog.md).
/// </summary>
public sealed class SpoonUnholyGrimoire : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_unholy_grimoire";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Damage", CurseRule.GrimoireDamage) };

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner != Owner || card.Type != CardType.Curse || Owner.Creature.CombatState is not { } combat) return;
		var enemies = combat.HittableEnemies.ToList();
		if (enemies.Count == 0) return;
		Flash();
		await CreatureCmd.Damage(choiceContext, enemies, CurseRule.GrimoireDamage, ValueProp.Unpowered, Owner.Creature);
	}
}

/// <summary>Every relic needs a pool; the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class UnholyGrimoirePoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<SpoonUnholyGrimoire>()).ToList();
	}
}
