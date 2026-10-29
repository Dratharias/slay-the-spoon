using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Silent;

public sealed class Sts1CorpseExplosion : Sts1SilentCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 6m),
	};

	public Sts1CorpseExplosion()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(3m);
	}
}
