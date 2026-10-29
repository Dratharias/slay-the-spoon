using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Relics;

/// <summary>StS1's Mutagenic Strength, from the Augmenter event only: Strength at combat start, gone after the first turn.</summary>
public sealed class Sts1MutagenicStrength : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	/// <summary>The mod ships no relic art: the icons come from Symbiotic Virus.</summary>
	protected override string IconBaseName => "symbiotic_virus";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<StrengthPower>(3m) };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			Flash();
			await PowerCmd.Apply<Sts1MutagenicStrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
				DynamicVars.Strength.BaseValue, Owner.Creature, null);
		}
	}
}

/// <summary>Every relic needs a pool (RelicModel.Pool); the event pool never feeds the grab bag, so seeds stay vanilla.</summary>
[HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
public static class MutagenicStrengthPoolPatch
{
	public static void Postfix(ref IEnumerable<RelicModel> __result)
	{
		__result = __result.Append(ModelDb.Relic<Sts1MutagenicStrength>()).ToList();
	}
}
