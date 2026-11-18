using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SlaySpoon.Cards.Sts1.Curses;

/// <summary>
/// StS1 Parasite also costs its max HP when transformed out of the deck. <see cref="CardCmd.Transform(IEnumerable{CardTransformation}, MegaCrit.Sts2.Core.Random.Rng?, CardPreviewStyle)"/>
/// never raises <c>BeforeCardRemoved</c> (that hook only fires for <c>RemoveFromDeck</c>, which keeps a plain removal
/// from counting twice), so the Parasites in the deck are noted before the transform and charged after it finishes.
/// </summary>
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Transform), typeof(IEnumerable<CardTransformation>),
	typeof(MegaCrit.Sts2.Core.Random.Rng), typeof(CardPreviewStyle))]
public static class ParasiteTransformPatch
{
	public static void Prefix(IEnumerable<CardTransformation> transformations, out List<Sts1Parasite>? __state)
	{
		var parasites = transformations.Select(t => t.Original).OfType<Sts1Parasite>()
			.Where(c => c.Pile?.Type == PileType.Deck).ToList();
		__state = parasites.Count == 0 ? null : parasites;
	}

	public static void Postfix(ref Task<IEnumerable<CardPileAddResult>> __result, List<Sts1Parasite>? __state)
	{
		if (__state == null) return;
		__result = ChargeAfter(__result, __state);
	}

	private static async Task<IEnumerable<CardPileAddResult>> ChargeAfter(Task<IEnumerable<CardPileAddResult>> transform,
		List<Sts1Parasite> parasites)
	{
		IEnumerable<CardPileAddResult> results = await transform;
		foreach (Sts1Parasite parasite in parasites)
		{
			await parasite.LoseMaxHpForLeaving();
		}
		return results;
	}
}
