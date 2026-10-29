using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1ThunderStrike : Sts1DefectCard
{
	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Move),
		new DynamicVar("Magic", 0m),
	};

	public Sts1ThunderStrike()
		: base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
