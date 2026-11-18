using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Curses;

/// <summary>
/// StS1 Pain, from the Ominous Forge's Rummage: Unplayable; while it sits in its owner's hand, each other card the owner
/// plays costs them 1 HP. StS2 has no Pain, so it lives in the hidden StS1 curse pool and nothing else hands it out.
/// </summary>
public sealed class Sts1Pain : Sts1CurseCard
{
	/// <summary>HP lost per other card played; a card var so the Cursed Deck table can raise it when upgraded.</summary>
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new HpLossVar(Sts1EventMath.PainHpLoss) };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Pile?.Type != PileType.Hand || cardPlay.Card == this || cardPlay.Card.Owner != Owner) return;
		await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.IntValue,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null);
	}
}
