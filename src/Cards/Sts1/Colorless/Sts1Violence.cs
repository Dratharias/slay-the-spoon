using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Violence: random Attacks from the draw pile, no choice.</summary>
public sealed class Sts1Violence : Sts1ColorlessCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(3),
	};

	public Sts1Violence()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> attacks = PileType.Draw.GetPile(Owner).Cards.Where(c => c.Type == CardType.Attack).ToList();
		for (int i = 0; i < DynamicVars.Cards.IntValue && attacks.Count > 0; i++)
		{
			CardModel card = Owner.RunState.Rng.CombatCardSelection.NextItem(attacks)!;
			attacks.Remove(card);
			await CardPileCmd.Add(card, PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
