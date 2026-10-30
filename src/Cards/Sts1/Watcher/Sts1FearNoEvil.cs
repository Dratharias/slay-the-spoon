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

/// <summary>StS1 Fear No Evil: enter Calm if the target intended to attack when the card was played.</summary>
public sealed class Sts1FearNoEvil : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<Sts1CalmStancePower>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
	};

	public Sts1FearNoEvil()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		// StS1 reads the intent before the hit lands.
		bool intendsToAttack = cardPlay.Target.Monster?.IntendsToAttack ?? false;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (intendsToAttack)
		{
			await StanceCmd.Enter(Owner, Stance.Calm, this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
