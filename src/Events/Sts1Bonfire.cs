using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Bonfire Spirits: offer a removable deck card; the reward follows its rarity (<see cref="Sts1EventMath.BonfireRewardFor"/>):
/// a Curse gives Spirit Poop (a Circlet when already held), Basic nothing, Common heals 5, Uncommon heals to full, Rare
/// gives 10 max HP and heals to full. StS1 offers no way out; here Leave shows only when no card can be offered.
/// </summary>
public sealed class Sts1Bonfire : Sts1Event
{
	private const string Key = "STS1_BONFIRE.pages.";

	public override Sts1EventKind Kind => Sts1EventKind.Bonfire;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Heal", Sts1EventMath.BonfireHeal),
		new DynamicVar("MaxHp", Sts1EventMath.BonfireMaxHp),
	};

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		new(this, Approach, Key + "INITIAL.options.CONTINUE"),
	};

	private Task Approach()
	{
		bool canOffer = PileType.Deck.GetPile(Owner!).Cards.Any(c => c?.IsRemovable ?? false);
		SetEventState(L10NLookup(Key + "CHOOSE.description"), canOffer
			? new EventOption[] { new(this, Offer, Key + "CHOOSE.options.OFFER", HoverTipFactory.FromRelic<Sts1SpiritPoop>()) }
			: new EventOption[]
			{
				new(this, null, Key + "CHOOSE.options.OFFER_LOCKED"),
				new(this, Leave, Key + "CHOOSE.options.LEAVE"),
			});
		return Task.CompletedTask;
	}

	private async Task Offer()
	{
		CardModel? card = (await CardSelectCmd.FromDeckForRemoval(Owner!,
			new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).FirstOrDefault();
		if (card == null)
		{
			SetEventFinished(L10NLookup(Key + "NOTHING.description"));
			return;
		}
		BonfireReward reward = Sts1EventMath.BonfireRewardFor(card.Rarity.ToString());
		await CardPileCmd.RemoveFromDeck(new[] { card });
		var creature = Owner!.Creature;
		switch (reward)
		{
			case BonfireReward.SpiritPoop:
				RelicModel relic = Owner.GetRelic<Sts1SpiritPoop>() == null ? ModelDb.Relic<Sts1SpiritPoop>() : ModelDb.Relic<Circlet>();
				await RelicCmd.Obtain(relic.ToMutable(), Owner);
				SetEventFinished(L10NLookup(Key + "CURSE.description"));
				return;
			case BonfireReward.Nothing:
				SetEventFinished(L10NLookup(Key + "NOTHING.description"));
				return;
			case BonfireReward.Heal:
				await CreatureCmd.Heal(creature, Sts1EventMath.BonfireHeal);
				SetEventFinished(L10NLookup(Key + "HEAL.description"));
				return;
			case BonfireReward.FullHeal:
				await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
				SetEventFinished(L10NLookup(Key + "FULL_HEAL.description"));
				return;
			default:
				await CreatureCmd.GainMaxHp(creature, Sts1EventMath.BonfireMaxHp);
				await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
				SetEventFinished(L10NLookup(Key + "MAX_HP.description"));
				return;
		}
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup(Key + "LEAVE.description"));
		return Task.CompletedTask;
	}
}
