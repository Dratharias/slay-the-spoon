using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Secret Technique: unplayable without a matching card in the draw pile; the upgrade removes Exhaust.</summary>
public sealed class Sts1SecretTechnique : Sts1ColorlessCard
{
	public override Sts1Relation Relation => Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.SecretTechnique);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override bool IsPlayable => PileType.Draw.GetPile(Owner).Cards.Any(c => c.Type == CardType.Skill);

	public Sts1SecretTechnique()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel? card = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(Owner), Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c.Type == CardType.Skill)).FirstOrDefault();
		if (card != null)
		{
			await CardPileCmd.Add(card, PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
