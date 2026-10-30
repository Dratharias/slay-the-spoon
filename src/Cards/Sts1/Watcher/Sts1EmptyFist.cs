using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Empty Fist: damage, then leave the current stance.</summary>
public sealed class Sts1EmptyFist : Sts1WatcherCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(9m, ValueProp.Move),
	};

	public Sts1EmptyFist()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target!)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
		await StanceCmd.Enter(Owner, Stance.Neutral, this, choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
