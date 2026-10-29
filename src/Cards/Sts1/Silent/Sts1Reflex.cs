using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Silent;

/// <summary>StS1 Reflex: Unplayable, draws when discarded by a card; StS2's is a Sly card that costs 3.</summary>
public sealed class Sts1Reflex : Sts1SilentCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(Reflex);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(2),
	};

	public Sts1Reflex()
		: base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
	{
	}

	// The game raises this hook for discards made by cards and effects, never for the end-of-turn hand discard.
	public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (card == this)
		{
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
