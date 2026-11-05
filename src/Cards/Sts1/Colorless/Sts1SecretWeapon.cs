using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Colorless;

/// <summary>StS1 Secret Weapon: unplayable without a matching card in the draw pile; the upgrade removes Exhaust.</summary>
public sealed class Sts1SecretWeapon : Sts1ColorlessCard
{
	/// <summary>Retired (owner, 2026-11-05): the StS2 card serves every mode; kept for old saves.</summary>
	public override Sts1Relation Relation => Sts1Relation.Retired;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.SecretWeapon);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override bool IsPlayable => PileType.Draw.GetPile(Owner).Cards.Any(c => c.Type == CardType.Attack);

	public Sts1SecretWeapon()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel? card = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(Owner), Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c.Type == CardType.Attack)).FirstOrDefault();
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
