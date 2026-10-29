using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Colorless;

public sealed class Sts1Apotheosis : Sts1ColorlessCard
{
	public override SlaySpoon.Core.Sts1Relation Relation => SlaySpoon.Core.Sts1Relation.Duplicate;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.Apotheosis);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Sts1Apotheosis()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
