using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Silent;

public sealed class Sts1SneakyStrike : Sts1SilentCard
{
	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(12m, ValueProp.Move),
	};

	public Sts1SneakyStrike()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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
