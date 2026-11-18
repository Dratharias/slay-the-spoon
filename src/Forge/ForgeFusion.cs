using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>
/// Which deck cards can be fused, and the fusion itself (spec "Fuse"). Copies are the same card id, or a StS1 card and its
/// StS2 namesake (every <see cref="Sts1Card"/> with a <see cref="Sts1Card.Sts2Counterpart"/>, whatever the run's mode).
/// </summary>
public static class ForgeFusion
{
	/// <summary>True while the fusion preview opens: the Hybrid swap, King Midas and Folie à deux leave that screen alone.</summary>
	public static bool Previewing { get; private set; }

	private static readonly Lazy<IReadOnlyDictionary<string, string>> Namesakes = new(BuildNamesakes);

	private static IReadOnlyDictionary<string, string> BuildNamesakes()
	{
		var pairs = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var type in typeof(Sts1Card).Assembly.GetTypes()
			         .Where(t => !t.IsAbstract && typeof(Sts1Card).IsAssignableFrom(t)))
		{
			try
			{
				if (ModelDb.GetById<CardModel>(ModelDb.GetId(type)) is not Sts1Card { Sts2Counterpart: { } sts2 } card) continue;
				string other = ModelDb.GetId(sts2).Entry;
				pairs[card.Id.Entry] = other;
				pairs.TryAdd(other, card.Id.Entry);
			}
			catch (Exception e)
			{
				SpoonLog.Error($"forge: namesake of {type.Name} unknown: {e.Message}");
			}
		}
		return pairs;
	}

	public static bool AreCopies(CardModel a, CardModel b) => a != b
		&& (a.Id == b.Id || Namesakes.Value.TryGetValue(a.Id.Entry, out var other) && other == b.Id.Entry);

	private static CardModel Canonical(CardModel card) => ModelDb.GetById<CardModel>(card.Id);

	/// <summary>Whether <paramref name="kept"/> at its level can become <paramref name="version"/> by fusion.</summary>
	private static bool CanBecome(CardModel kept, CardModel version)
	{
		var canonical = Canonical(version);
		return ForgeLevels.Fusable(canonical) && ForgeRule.CanFuseInto(kept.CurrentUpgradeLevel, ForgeLevels.Limit(canonical), fusable: true);
	}

	/// <summary>The versions the result can take: the kept card's own, then the copy's when it is the other version.</summary>
	public static IReadOnlyList<CardModel> Versions(CardModel kept, CardModel sacrificed)
	{
		var versions = new List<CardModel>();
		if (CanBecome(kept, kept)) versions.Add(kept);
		if (sacrificed.Id != kept.Id && CanBecome(kept, sacrificed)) versions.Add(sacrificed);
		return versions;
	}

	public static bool CanFuse(CardModel kept, CardModel sacrificed) => AreCopies(kept, sacrificed) && Versions(kept, sacrificed).Count > 0;

	public static bool HasPartner(CardModel kept, IReadOnlyList<CardModel> deck) => deck.Any(c => CanFuse(kept, c));

	public static bool CanFuseAny(Player player)
	{
		var deck = PileType.Deck.GetPile(player).Cards;
		return deck.Any(c => HasPartner(c, deck));
	}

	public static int ResultLevel(CardModel kept, CardModel sacrificed, CardModel version) =>
		ForgeRule.FusedLevel(kept.CurrentUpgradeLevel, sacrificed.CurrentUpgradeLevel, ForgeLevels.Limit(Canonical(version)));

	/// <summary>
	/// A new card of <paramref name="version"/> at the result level, carrying the kept card's enchantment when it can.
	/// The enchantment goes on first, then the levels, in the order a reload replays them.
	/// </summary>
	public static CardModel Build(CardModel kept, CardModel sacrificed, CardModel version)
	{
		var card = kept.CardScope!.CreateCard(Canonical(version), kept.Owner);
		if (kept.Enchantment is { } enchantment)
		{
			var copy = ModelDb.GetById<EnchantmentModel>(enchantment.Id).ToMutable();
			if (copy.CanEnchant(card)) CardCmd.Enchant(copy, card, enchantment.Amount);
		}
		ForgeLevels.RaiseTo(card, ResultLevel(kept, sacrificed, version));
		return card;
	}

	/// <summary>The preview screen: one card per allowed version, skippable (skip cancels the whole fusion).</summary>
	public static Task<CardModel?> Preview(Player player, IReadOnlyList<CardModel> previews)
	{
		Previewing = true;
		try
		{
			return CardSelectCmd.FromChooseACardScreen(new MegaCrit.Sts2.Core.GameActions.Multiplayer.BlockingPlayerChoiceContext(),
				previews, player, canSkip: true);
		}
		finally
		{
			Previewing = false;
		}
	}

	/// <summary>
	/// Applies the fusion: the kept card rises in place, or becomes the chosen other version (a transform, so the deck order
	/// holds); the copy leaves the deck like a removal.
	/// </summary>
	public static async Task<CardModel> Apply(CardModel kept, CardModel sacrificed, CardModel version)
	{
		CardModel result;
		if (version.Id == kept.Id)
		{
			ForgeLevels.RaiseTo(kept, ResultLevel(kept, sacrificed, version));
			result = kept;
		}
		else
		{
			result = Build(kept, sacrificed, version);
			result = (await CardCmd.Transform(kept, result, CardPreviewStyle.None))?.cardAdded ?? result;
		}
		await CardPileCmd.RemoveFromDeck(sacrificed, showPreview: false);
		SpoonLog.Info($"forge: {kept.Id.Entry}+{kept.CurrentUpgradeLevel} + {sacrificed.Id.Entry}+{sacrificed.CurrentUpgradeLevel} -> {result.Id.Entry}+{result.CurrentUpgradeLevel}");
		return result;
	}
}
