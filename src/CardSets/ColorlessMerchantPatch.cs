using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.CardSets;

/// <summary>Apply vanilla colorless pricing and purchase history to our ordinary hidden pool too.</summary>
[HarmonyPatch]
public static class ColorlessMerchantPatch
{
	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AccessTools.Method(typeof(MerchantCardEntry), "GetCost");
		yield return EventCardGrantPatch.AsyncBody(typeof(MerchantCardEntry), "OnTryPurchase");
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Isinst && Equals(instruction.operand, typeof(ColorlessCardPool)))
			{
				// Preserve the original reference/null test, including any subsequent ldnull/cgt.un sequence.
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(ColorlessMerchantPatch), nameof(OrdinaryColorlessPool));
				replaced++;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			throw new InvalidOperationException($"Expected one merchant colorless pool check, found {replaced}");
		}
	}

	public static CardPoolModel? OrdinaryColorlessPool(CardPoolModel pool) =>
		pool is ColorlessCardPool or Sts1ColorlessCardPool ? pool : null;
}
