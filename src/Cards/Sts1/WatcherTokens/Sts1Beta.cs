using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.WatcherTokens;

public sealed class Sts1Beta : Sts1WatcherTokenCard
{
	public Sts1Beta() : base(2, CardType.Skill, TargetType.Self) { }
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Sts1Omega>() };

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel omega = CombatState!.CreateCard<Sts1Omega>(Owner);
		await CardPileCmd.AddGeneratedCardToCombat(omega, PileType.Draw, Owner, CardPilePosition.Random);
	}
	protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
