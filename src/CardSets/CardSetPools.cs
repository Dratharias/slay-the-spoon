using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Relics;

namespace SlaySpoon.CardSets;

/// <summary>Builds a character's card pool for a player's card set mode.</summary>
public static class CardSetPools
{
	/// <summary>A vanilla ordinary pool, its StS1 pool and the vanilla ids that have a StS1 namesake.</summary>
	private sealed record Pair(Type Vanilla, Type Sts1, string[] Matched);

	private static readonly Pair[] Pairs =
	{
		new(typeof(IroncladCardPool), typeof(Sts1IroncladCardPool), Sts1Matches.Ironclad),
		new(typeof(SilentCardPool), typeof(Sts1SilentCardPool), Sts1Matches.Silent),
		new(typeof(DefectCardPool), typeof(Sts1DefectCardPool), Sts1Matches.Defect),
		new(typeof(ColorlessCardPool), typeof(Sts1ColorlessCardPool), Sts1Matches.Colorless),
	};

	/// <summary>The run's card set mode, as ticked in Custom Run (none = StS2 only).</summary>
	public static CardSetMode ModeOf(IRunState? run)
	{
		return run?.Modifiers.OfType<SpoonCardSet>().FirstOrDefault()?.Mode ?? CardSetMode.Sts2Only;
	}

	/// <summary>
	/// The mode <paramref name="player"/>'s card sources follow: the run's, or Hybrid while they hold the Spire
	/// Souvenir. Read live, so losing the relic ends the effect. With no player, the mode of <paramref name="fallbackRun"/>.
	/// </summary>
	public static CardSetMode ModeFor(Player? player, IRunState? fallbackRun = null)
	{
		if (player == null)
		{
			return ModeOf(fallbackRun);
		}
		return SouvenirRule.EffectiveMode(ModeOf(player.RunState), player.Relics.Any(r => r is SpoonSpireSouvenir));
	}

	/// <summary>
	/// The player a GetUnlockedCards call is for. Each player owns a distinct UnlockState and vanilla callers pass
	/// theirs; the run-wide RunState.UnlockState (Massive Scroll) matches nobody.
	/// </summary>
	public static Player? PlayerOf(IRunState? run, UnlockState unlockState)
	{
		return run?.Players.FirstOrDefault(p => ReferenceEquals(p.UnlockState, unlockState));
	}

	/// <summary>True when <paramref name="character"/> has StS1 cards, so the Spire Souvenir does something for them.</summary>
	public static bool HasSts1Cards(CharacterModel character)
	{
		return HasPair(character.CardPool);
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
		IEnumerable<CardModel> cards = Pairs.SelectMany(pair =>
			ModelDb.GetById<CardPoolModel>(ModelDb.GetId(pair.Sts1)).AllCards)
			.Concat(ModelDb.CardPool<Sts1EventCardPool>().AllCards);
		foreach (Sts1Card card in cards.OfType<Sts1Card>())
		{
			byId[card.Id.Entry] = card;
			if (card.Sts2Counterpart != null)
			{
				CardModel sts2 = ModelDb.GetById<CardModel>(ModelDb.GetId(card.Sts2Counterpart));
				byId[sts2.Id.Entry] = sts2;
			}
			entries.Add(EntryOf(card));
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
