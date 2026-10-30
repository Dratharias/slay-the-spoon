using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Watcher;

public sealed class Sts1DevaForm : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 1m),
		new EnergyVar(1),
	};

	public Sts1DevaForm()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		throw new NotImplementedException("STS1 port");
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Ethereal);
	}
}
