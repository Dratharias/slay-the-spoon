using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using SlaySpoon.Cards.Sts1.Colorless;
using SlaySpoon.Core;

namespace SlaySpoon.Events;

/// <summary>StS1's Vampires(?): every basic Strike becomes 5 Bites, paid with max HP or with a Blood Vial.</summary>
public sealed class Sts1Vampires : Sts1Event
{
	private const int Bites = 5;

	public override Sts1EventKind Kind => Sts1EventKind.Vampires;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("MaxHpLoss", 0m),
		new DynamicVar("Bites", Bites),
	};

	public override void CalculateVars()
	{
		DynamicVars["MaxHpLoss"].BaseValue = Sts1EventRule.VampiresMaxHpLoss(Owner!.Creature.MaxHp);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> options = new();
		if (CardUnlocked)
		{
			options.Add(new EventOption(this, Accept, "STS1_VAMPIRES.pages.INITIAL.options.ACCEPT",
				HoverTipFactory.FromCardWithCardHoverTips<Sts1Bite>()).ThatDecreasesMaxHp(DynamicVars["MaxHpLoss"].BaseValue));
			if (Owner!.GetRelic<BloodVial>() != null)
			{
				options.Add(new EventOption(this, OfferVial, "STS1_VAMPIRES.pages.INITIAL.options.BLOOD_VIAL",
					HoverTipFactory.FromCardWithCardHoverTips<Sts1Bite>()));
			}
		}
		else
		{
			options.Add(new EventOption(this, null, "STS1_VAMPIRES.pages.INITIAL.options.ACCEPT_LOCKED"));
		}
		options.Add(new EventOption(this, Refuse, "STS1_VAMPIRES.pages.INITIAL.options.REFUSE"));
		return options;
	}

	private async Task Accept()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHpLoss"].BaseValue, isFromCard: false);
		await TradeStrikesForBites();
		SetEventFinished(L10NLookup("STS1_VAMPIRES.pages.ACCEPT.description"));
	}

	private async Task OfferVial()
	{
		BloodVial? vial = Owner!.GetRelic<BloodVial>();
		if (vial != null)
		{
			await RelicCmd.Remove(vial);
		}
		await TradeStrikesForBites();
		SetEventFinished(L10NLookup("STS1_VAMPIRES.pages.BLOOD_VIAL.description"));
	}

	private Task Refuse()
	{
		SetEventFinished(L10NLookup("STS1_VAMPIRES.pages.REFUSE.description"));
		return Task.CompletedTask;
	}

	private async Task TradeStrikesForBites()
	{
		Player owner = Owner!;
		List<CardModel> strikes = PileType.Deck.GetPile(owner).Cards
			.Where(c => c.Tags.Contains(CardTag.Strike) && c.Rarity == CardRarity.Basic && c.IsRemovable)
			.ToList();
		if (strikes.Count > 0)
		{
			await CardPileCmd.RemoveFromDeck(strikes);
		}
		List<CardPileAddResult> added = new();
		for (int i = 0; i < Bites; i++)
		{
			added.Add(await CardPileCmd.Add(owner.RunState.CreateCard<Sts1Bite>(owner), PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(added, 2f);
	}
}
