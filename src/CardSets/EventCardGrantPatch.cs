using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Direct grants bypass CardFactory. Replace only these four source call sites: replacing CreateCard globally
/// would break its generic return cast, clones, loads and explicit console requests.
/// </summary>
[HarmonyPatch]
public static class EventCardGrantPatch
{
	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AsyncBody(typeof(SpiritGrafter), "LetItIn");
		yield return AsyncBody(typeof(ZenWeaver), "BreathingTechniques");
		yield return AsyncBody(typeof(JewelryBox), nameof(JewelryBox.AfterObtained));
		yield return AsyncBody(typeof(DistinguishedCape), nameof(DistinguishedCape.AfterObtained));
	}

	internal static MethodInfo AsyncBody(Type type, string method) => AccessTools.Method(
		AccessTools.Method(type, method).GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType, "MoveNext");

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.operand is MethodInfo method && method.IsGenericMethod &&
				method.Name == nameof(ICardScope.CreateCard) &&
				method.DeclaringType == typeof(ICardScope))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(EventCardGrantPatch), nameof(Create))
					.MakeGenericMethod(method.GetGenericArguments());
				replaced++;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			throw new InvalidOperationException($"Expected one event card creation call, found {replaced}");
		}
	}

	private static Sts1Card? Counterpart(CardModel vanilla) => ModelDb.CardPool<Sts1EventCardPool>()
		.AllCards.OfType<Sts1Card>().FirstOrDefault(c => c.Sts2Counterpart == vanilla.GetType());

	public static CardModel Create<T>(ICardScope scope, Player owner) where T : CardModel
	{
		CardModel vanilla = ModelDb.Card<T>();
		Sts1Card? sts1 = Counterpart(vanilla);
		CardSetMode mode = CardSetPools.ModeFor(owner);
		// Consume RNG only for an actual Hybrid duplicate, once per granted card on every client.
		bool roll = mode == CardSetMode.Hybrid && sts1?.Relation == Sts1Relation.Duplicate &&
			owner.RunState.Rng.Niche.NextBool();
		CardModel selected = sts1 != null && ColorlessCardRules.UseSts1EventVersion(mode, sts1.Relation, roll)
			? sts1 : vanilla;
		return scope.CreateCard(selected, owner);
	}

	/// <summary>Preview both Hybrid versions without consuming RNG; otherwise preview the granted version.</summary>
	public static IEnumerable<IHoverTip> Preview<T>(bool upgrade, AbstractModel source) where T : CardModel
	{
		Player? player = source switch
		{
			EventModel evt => evt.Owner,
			RelicModel relic when relic.IsMutable => relic.Owner,
			_ => null,
		};
		CardModel vanilla = ModelDb.Card<T>();
		Sts1Card? sts1 = Counterpart(vanilla);
		CardSetMode mode = CardSetPools.ModeFor(player);
		var cards = new List<CardModel>();
		if (sts1 != null && ColorlessCardRules.UseSts1EventVersion(mode, sts1.Relation, true))
		{
			cards.Add(sts1);
			if (mode == CardSetMode.Hybrid && sts1.Relation == Sts1Relation.Duplicate)
			{
				cards.Add(vanilla);
			}
		}
		else
		{
			cards.Add(vanilla);
		}
		return cards.SelectMany(c => new[] { HoverTipFactory.FromCard(c, upgrade) }.Concat(c.HoverTips));
	}
}

[HarmonyPatch]
public static class EventCardPreviewPatch
{
	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AccessTools.Method(typeof(SpiritGrafter), "GenerateInitialOptions");
		yield return AccessTools.Method(typeof(ZenWeaver), "GenerateInitialOptions");
		yield return AccessTools.PropertyGetter(typeof(JewelryBox), "ExtraHoverTips");
		yield return AccessTools.PropertyGetter(typeof(DistinguishedCape), "ExtraHoverTips");
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.operand is MethodInfo method && method.IsGenericMethod &&
				method.DeclaringType == typeof(HoverTipFactory) &&
				method.Name == nameof(HoverTipFactory.FromCardWithCardHoverTips))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(EventCardGrantPatch), nameof(EventCardGrantPatch.Preview))
					.MakeGenericMethod(method.GetGenericArguments());
				replaced++;
			}
			yield return instruction;
		}
		if (replaced != 1)
		{
			throw new InvalidOperationException($"Expected one event card preview call, found {replaced}");
		}
	}
}
