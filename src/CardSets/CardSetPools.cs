using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.CardSets;

/// <summary>Builds a character's card pool for the run's card set mode.</summary>
public static class CardSetPools
{
	/// <summary>A vanilla character pool, its StS1 pool and the vanilla ids that have a StS1 namesake.</summary>
	private sealed record Pair(Type Vanilla, Type Sts1, string[] Matched);

	private static readonly Pair[] Pairs =
	{
		new(typeof(IroncladCardPool), typeof(Sts1IroncladCardPool), Sts1Matches.Ironclad),
	};

	public static CardSetMode ModeOf(IRunState? run)
	{
		return run?.Modifiers.OfType<SpoonCardSet>().FirstOrDefault()?.Mode ?? CardSetMode.Sts2Only;
	}

	/// <summary>True when <paramref name="pool"/> is a vanilla pool or StS1 pool that the mode reshapes.</summary>
	public static bool HasPair(CardPoolModel pool)
	{
		return PairOf(pool) != null;
	}

	/// <param name="vanillaUnlocked">Calls the game's own GetUnlockedCards on a pool (no mode applied).</param>
	public static IReadOnlyList<CardModel> Compose(CardPoolModel pool, CardSetMode mode,
		Func<CardPoolModel, IEnumerable<CardModel>> vanillaUnlocked)
	{
		Pair pair = PairOf(pool) ?? throw new ArgumentException($"{pool.Id} has no StS1 pair");
		List<CardModel> vanilla = vanillaUnlocked(ModelDb.GetById<CardPoolModel>(ModelDb.GetId(pair.Vanilla))).ToList();
		List<CardModel> sts1 = vanillaUnlocked(ModelDb.GetById<CardPoolModel>(ModelDb.GetId(pair.Sts1))).ToList();

		Dictionary<string, CardModel> byId = vanilla.Concat(sts1).GroupBy(c => c.Id.Entry)
			.ToDictionary(g => g.Key, g => g.First());
		Sts1Entry[] entries = sts1.OfType<Sts1Card>().Select(EntryOf).ToArray();

		return CardSetComposer.Compose(mode, vanilla.Select(c => c.Id.Entry).ToList(), pair.Matched, entries)
			.Select(id => byId[id])
			.ToList();
	}

	/// <summary>The other version of a duplicate card (canonical), or null when <paramref name="card"/> has none.</summary>
	public static CardModel? PartnerOf(CardModel card)
	{
		return Partners.Value.GetValueOrDefault(card.Id.Entry);
	}

	private static readonly Lazy<IReadOnlyDictionary<string, CardModel>> Partners = new(BuildPartners);

	private static IReadOnlyDictionary<string, CardModel> BuildPartners()
	{
		var byId = new Dictionary<string, CardModel>();
		var entries = new List<Sts1Entry>();
		foreach (Pair pair in Pairs)
		{
			foreach (Sts1Card card in ModelDb.GetById<CardPoolModel>(ModelDb.GetId(pair.Sts1)).AllCards.OfType<Sts1Card>())
			{
				byId[card.Id.Entry] = card;
				if (card.Sts2Counterpart != null)
				{
					CardModel sts2 = ModelDb.GetById<CardModel>(ModelDb.GetId(card.Sts2Counterpart));
					byId[sts2.Id.Entry] = sts2;
				}
				entries.Add(EntryOf(card));
			}
		}
		return VersionPairs.Of(entries).ToDictionary(kv => kv.Key, kv => byId[kv.Value]);
	}

	private static Sts1Entry EntryOf(Sts1Card card)
	{
		return new Sts1Entry(card.Id.Entry,
			card.Sts2Counterpart == null ? null : ModelDb.GetId(card.Sts2Counterpart).Entry,
			card.Relation);
	}

	private static Pair? PairOf(CardPoolModel pool)
	{
		Type type = pool.GetType();
		return Pairs.FirstOrDefault(p => p.Vanilla == type || p.Sts1 == type);
	}
}
