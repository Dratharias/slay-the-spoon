using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Madness: prefers a hand card that costs energy right now, else one whose printed cost is above 0.</summary>
public sealed class Sts1Madness : Sts1ColorlessCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Sts1Madness()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> hand = PileType.Hand.GetPile(Owner).Cards.Where(c => !c.EnergyCost.CostsX).ToList();
		List<CardModel> candidates = hand.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.All) > 0).ToList();
		if (candidates.Count == 0)
		{
			candidates = hand.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.None) > 0).ToList();
		}
		CardModel? card = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
		card?.EnergyCost.SetThisCombat(0);
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
