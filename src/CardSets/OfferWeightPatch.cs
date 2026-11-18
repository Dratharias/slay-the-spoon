using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Achilles;
using SlaySpoon.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>
/// Weighted card draws (<see cref="OfferWeights"/>): the Souvenir lean and the cross-game combo weight. The factory's
/// single uniform <c>Rng.NextItem</c> in card rewards, merchants, combat generation (Calamity-like effects) and
/// transforms is replaced by <see cref="Pick"/>, which still makes exactly one draw on the same RNG, so co-op peers
/// stay in step and later draws keep their counters. With equal weights it calls the vanilla draw unchanged.
/// Distinct combat generation (Discovery-like effects) draws several cards at once through <c>TakeRandom</c> and stays
/// uniform.
/// </summary>
[HarmonyPatch]
public static class OfferWeightPatch
{
	[ThreadStatic] private static Player? _player;

	private static readonly MethodInfo NextItem =
		AccessTools.Method(typeof(Rng), nameof(Rng.NextItem)).MakeGenericMethod(typeof(CardModel));

	private static IEnumerable<MethodBase> TargetMethods()
	{
		Type f = typeof(CardFactory);
		yield return AccessTools.Method(f, "CreateForReward", new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardCreationOptions) });
		yield return AccessTools.Method(f, nameof(CardFactory.CreateForMerchant), new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType) });
		yield return AccessTools.Method(f, nameof(CardFactory.CreateForMerchant), new[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardRarity) });
		yield return AccessTools.Method(f, nameof(CardFactory.GetForCombat));
		yield return AccessTools.Method(f, nameof(CardFactory.CreateRandomCardForTransform), new[] { typeof(CardModel), typeof(bool), typeof(Rng) });
		yield return AccessTools.Method(f, nameof(CardFactory.CreateRandomCardForTransform), new[] { typeof(CardModel), typeof(IEnumerable<CardModel>), typeof(bool), typeof(Rng) });
	}

	/// <summary>The first argument is the player, or the card being transformed (its owner).</summary>
	private static void Prefix(object[] __args, out Player? __state)
	{
		__state = _player;
		_player = __args.Length > 0 ? __args[0] as Player ?? (__args[0] as CardModel)?.Owner : null;
	}

	private static void Finalizer(Player? __state) => _player = __state;

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var pick = AccessTools.Method(typeof(OfferWeightPatch), nameof(Pick));
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(NextItem))
			{
				replaced++;
				yield return new CodeInstruction(OpCodes.Call, pick).MoveLabelsFrom(instruction);
				continue;
			}
			yield return instruction;
		}
		if (replaced == 0) SpoonLog.Error("cards: offer weights found no NextItem call to replace");
	}

	/// <summary>Stands in for <c>rng.NextItem(items)</c>; never throws past what vanilla would.</summary>
	public static CardModel? Pick(Rng rng, IEnumerable<CardModel> items)
	{
		CardModel[] cards = items as CardModel[] ?? items.ToArray();
		double[]? weights = null;
		try
		{
			weights = Weights(_player, cards);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"cards: offer weights failed, vanilla draw used: {e}");
		}
		if (weights == null || OfferWeights.IsUniform(weights)) return rng.NextItem(cards);
		return cards[OfferWeights.Pick(weights, rng.NextDouble())];
	}

	private static double[]? Weights(Player? player, CardModel[] cards)
	{
		if (player == null || cards.Length < 2) return null;
		var completers = OfferWeights.ComboCompleters(
			player.RunState.Players.SelectMany(p => p.Deck.Cards).Select(c => c.Id.Entry).ToHashSet());
		OfferSide? lean = VersionSwap.IsHybrid(player) ? Lean(player) : null;
		HashSet<string>? blockCards = player.Relics.Any(r => r is SpoonAchillesHeel)
			? cards.Where(AchillesBlock.IsBlockCard).Select(c => c.Id.Entry).ToHashSet()
			: null;
		if (completers.Count == 0 && lean == null && blockCards == null) return null;
		return OfferWeights.Compute(cards.Select(c => (c.Id.Entry, SideOf(c))).ToList(), lean, completers, blockCards);
	}

	/// <summary>The run's lean, from its seed on a private RNG that no vanilla draw shares.</summary>
	private static OfferSide Lean(Player player) =>
		new Rng(player.RunState.Rng.Seed, "SlaySpoon.SouvenirLean").NextBool() ? OfferSide.Sts1 : OfferSide.Sts2;

	private static OfferSide SideOf(CardModel card)
	{
		if (card is Sts1Card) return OfferSide.Sts1;
		return CardSetPools.PartnerOf(card) != null ? OfferSide.Neutral : OfferSide.Sts2;
	}
}
