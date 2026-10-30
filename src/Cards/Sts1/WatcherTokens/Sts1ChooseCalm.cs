using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.WatcherTokens;

public sealed class Sts1ChooseCalm : Sts1WatcherTokenCard
{
	public Sts1ChooseCalm() : base(0, CardType.Status, TargetType.Self) { }
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1CalmStancePower>() };
	public async Task ApplyChoice(PlayerChoiceContext choiceContext, CardModel? source = null)
	{
		await StanceCmd.Enter(Owner, Stance.Calm, source, choiceContext);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await ApplyChoice(choiceContext, this);
	}
}
