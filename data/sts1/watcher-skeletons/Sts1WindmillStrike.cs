using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Watcher;

public sealed class Sts1WindmillStrike : Sts1WatcherCard
{
	protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Move),
		new DynamicVar("Magic", 4m),
	};

	public Sts1WindmillStrike()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}
}
