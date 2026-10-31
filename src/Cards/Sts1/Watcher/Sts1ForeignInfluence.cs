using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Foreign Influence: choose 1 of 3 Attacks from the card set mode's pools; upgraded, it costs 0 this turn.</summary>
public sealed class Sts1ForeignInfluence : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Sts1ForeignInfluence()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> options = BuildOptions();
		if (options.Count == 0)
		{
			return;
		}
		CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: false);
		if (chosen == null)
		{
			return;
		}
		if (IsUpgraded)
		{
			chosen.SetToFreeThisTurn();
		}
		// StS1 ForeignInfluenceAction inserts its selection directly, bypassing Master Reality.
		Sts1MasterRealityPower.ExcludeForeignInfluence(chosen);
		await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
	}

	protected override void OnUpgrade()
	{
	}

	/// <summary>
	/// Three distinct Attacks from every character pool the owner's card set mode allows: StS1-only reaches the StS1 pools
	/// (including the Watcher's); Hybrid adds the StS2 characters without StS1 cards. Each pick rolls its rarity
	/// 55/30/15 on the combat card generation Rng, as StS1 does, over a fixed pool order so co-op peers agree.
	/// </summary>
	private List<CardModel> BuildOptions()
	{
		CardSetMode mode = CardSetPools.ModeFor(Owner);
		IEnumerable<CardPoolModel> pools = ModelDb.AllCharacterCardPools.Append(ModelDb.CardPool<Sts1WatcherCardPool>());
		List<CardModel> candidates = pools
			.Where(pool => mode switch
			{
				CardSetMode.Sts1Only => CardSetPools.HasPair(pool) || pool is Sts1WatcherCardPool,
				CardSetMode.Hybrid => true,
				_ => pool is not Sts1WatcherCardPool,
			})
			.SelectMany(pool => pool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
			.Where(card => card.Type == CardType.Attack)
			.Distinct()
			.OrderBy(card => card.Id.Entry, StringComparer.Ordinal)
			.ToList();
		candidates = CardFactory.FilterForCombat(candidates).ToList();
		candidates = candidates.Where(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare).ToList();
		Rng rng = Owner.RunState.Rng.CombatCardGeneration;
		var picked = new List<CardModel>();
		while (picked.Count < 3 && candidates.Count > 0)
		{
			CardRarity wanted = WatcherSkillMath.RarityFromRoll(rng.NextInt(100)) switch
			{
				ForeignRarity.Common => CardRarity.Common,
				ForeignRarity.Uncommon => CardRarity.Uncommon,
				_ => CardRarity.Rare,
			};
			List<CardModel> bucket = candidates.Where(card => card.Rarity == wanted).ToList();
			CardModel pick = rng.NextItem(bucket.Count > 0 ? bucket : candidates)!;
			candidates.RemoveAll(card => card.Id.Entry == pick.Id.Entry);
			picked.Add(CombatState!.CreateCard(pick, Owner));
		}
		return picked;
	}
}
