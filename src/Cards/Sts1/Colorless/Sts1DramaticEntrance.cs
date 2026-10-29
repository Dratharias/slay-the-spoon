using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Colorless;

public sealed class Sts1DramaticEntrance : Sts1ColorlessCard
{
	public override SlaySpoon.Core.Sts1Relation Relation => SlaySpoon.Core.Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(MegaCrit.Sts2.Core.Models.Cards.DramaticEntrance);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, CardKeyword.Innate };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
	};

	public Sts1DramaticEntrance()
		: base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
