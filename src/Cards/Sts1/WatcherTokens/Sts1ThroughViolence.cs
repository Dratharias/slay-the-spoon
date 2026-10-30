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

public sealed class Sts1ThroughViolence : Sts1WatcherTokenCard
{
	public Sts1ThroughViolence() : base(0, CardType.Attack, TargetType.AnyEnemy) { }
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(20m, ValueProp.Move) };

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target!).Execute(choiceContext);
	}
	protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(10m); }
}
