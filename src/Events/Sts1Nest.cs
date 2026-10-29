using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Colorless;
using SlaySpoon.Core;

namespace SlaySpoon.Events;

/// <summary>StS1's The Nest: rob the donation box, or bleed for the cultists' Ritual Dagger.</summary>
public sealed class Sts1Nest : Sts1Event
{
	public override Sts1EventKind Kind => Sts1EventKind.Nest;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new GoldVar(99),
		new DynamicVar("HpLoss", 6m),
	};

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new[]
		{
			new EventOption(this, SmashAndGrab, "STS1_NEST.pages.INITIAL.options.SMASH_AND_GRAB"),
			CardUnlocked
				? new EventOption(this, StayInLine, "STS1_NEST.pages.INITIAL.options.STAY_IN_LINE",
					HoverTipFactory.FromCardWithCardHoverTips<Sts1RitualDagger>()).ThatDoesDamage(DynamicVars["HpLoss"].BaseValue)
				: new EventOption(this, null, "STS1_NEST.pages.INITIAL.options.STAY_IN_LINE_LOCKED"),
		};
	}

	private async Task SmashAndGrab()
	{
		await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
		SetEventFinished(L10NLookup("STS1_NEST.pages.SMASH_AND_GRAB.description"));
	}

	private async Task StayInLine()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["HpLoss"].BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		CardModel dagger = Owner.RunState.CreateCard<Sts1RitualDagger>(Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(dagger, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("STS1_NEST.pages.STAY_IN_LINE.description"));
	}
}
