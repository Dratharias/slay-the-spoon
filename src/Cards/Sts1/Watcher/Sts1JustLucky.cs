using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Watcher;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Just Lucky: Scry, Block, damage, in that order.</summary>
public sealed class Sts1JustLucky : Sts1WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { WatcherTips.Scry() };

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(3m, ValueProp.Move),
		new BlockVar(2m, ValueProp.Move),
		new DynamicVar("Magic", 1m),
	};

	public Sts1JustLucky()
		: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await ScryCmd.Scry(Owner, DynamicVars["Magic"].IntValue, choiceContext);
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
		DynamicVars.Block.UpgradeValueBy(1m);
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}
}
