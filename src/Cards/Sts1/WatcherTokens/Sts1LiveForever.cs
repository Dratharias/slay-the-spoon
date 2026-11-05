using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.WatcherTokens;

public sealed class Sts1LiveForever : Sts1WatcherTokenCard
{
	public Sts1LiveForever() : base(0, CardType.Power, TargetType.Self) { }
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Magic", 6m) };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1PlatedArmorPower>() };
	public async Task ApplyChoice(PlayerChoiceContext choiceContext, CardModel? source = null)
	{
		await PowerCmd.Apply<Sts1PlatedArmorPower>(choiceContext, Owner.Creature, DynamicVars["Magic"].BaseValue, Owner.Creature, source);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await ApplyChoice(choiceContext, this);
	}
	protected override void OnUpgrade() { DynamicVars["Magic"].UpgradeValueBy(2m); }
}
