using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Massive Scroll uses run-wide unlocks and requires co-op-only cards. Rebuild its options for the owner;
/// StS1 has no co-op-only cards, so fall back to ordinary allowed cards when that filter leaves none.
/// </summary>
[HarmonyPatch]
public static class MassiveScrollPatch
{
	public static MethodBase TargetMethod() => EventCardGrantPatch.AsyncBody(typeof(MassiveScroll), nameof(MassiveScroll.AfterObtained));

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo factory = AccessTools.Method(typeof(CardFactory), nameof(CardFactory.CreateForReward),
			new[] { typeof(Player), typeof(int), typeof(CardCreationOptions) });
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(factory))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(MassiveScrollPatch), nameof(CreateReward));
				replaced++;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			throw new InvalidOperationException($"Expected one Massive Scroll reward call, found {replaced}");
		}
	}

	public static IEnumerable<CardCreationResult> CreateReward(Player player, int count, CardCreationOptions options)
	{
		if (CardSetPools.ModeFor(player) == CardSetMode.Sts2Only)
		{
			return CardFactory.CreateForReward(player, count, options);
		}
		IEnumerable<CardModel> ordinary = ModelDb.CardPool<ColorlessCardPool>()
			.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			.Concat(player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
			.Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare);
		IReadOnlyList<CardModel> eligible = ColorlessCardRules.PreferCoopCards(ordinary.ToList(),
			c => c.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly);
		return CardFactory.CreateForReward(player, count, options.WithCustomPool(eligible));
	}
}
