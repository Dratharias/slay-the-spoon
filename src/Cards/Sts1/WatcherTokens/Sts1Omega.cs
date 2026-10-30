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

public sealed class Sts1Omega : Sts1WatcherTokenCard
{
	public Sts1Omega() : base(3, CardType.Power, TargetType.Self) { }
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Magic", 50m) };
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<Sts1OmegaPower>() };

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<Sts1OmegaPower>(choiceContext, Owner.Creature, DynamicVars["Magic"].BaseValue, Owner.Creature, this);
	}
	protected override void OnUpgrade() { DynamicVars["Magic"].UpgradeValueBy(10m); }
}
