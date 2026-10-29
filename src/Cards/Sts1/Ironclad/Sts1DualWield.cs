using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Ironclad;

/// <summary>StS1 Dual Wield, same as StS2's; kept because StS2's is an Event card outside the Ironclad pool.</summary>
public sealed class Sts1DualWield : Sts1IroncladCard
{
	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(DualWield);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(1),
	};

	public Sts1DualWield()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel? selection = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
			context: choiceContext, player: Owner, filter: c => c.Type is CardType.Attack or CardType.Power, source: this))
			.FirstOrDefault();
		if (selection == null)
		{
			return;
		}
		for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
		{
			await CardPileCmd.AddGeneratedCardToCombat(selection.CreateClone(), PileType.Hand, Owner);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
