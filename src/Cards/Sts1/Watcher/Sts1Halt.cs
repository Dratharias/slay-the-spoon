using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Halt: Block, plus the Magic amount again while the owner is in Wrath (one gain, with Dexterity applied to both amounts as in StS1).</summary>
public sealed class Sts1Halt : Sts1WatcherCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<Sts1WrathStancePower>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(3m, ValueProp.Move),
		new BlockVar("Magic", 9m, ValueProp.Move),
	};

	public Sts1Halt()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// HaltAction combines two independently modified amounts into one GainBlockAction.
		decimal block = Hook.ModifyBlock(CombatState!, Owner.Creature, DynamicVars.Block.BaseValue,
			DynamicVars.Block.Props, this, cardPlay, out var modifiers);
		var affected = modifiers.ToList();
		decimal extra = 0;
		if (StanceCmd.Current(Owner) == Stance.Wrath)
		{
			extra = Hook.ModifyBlock(CombatState!, Owner.Creature, DynamicVars["Magic"].BaseValue,
				DynamicVars.Block.Props, this, cardPlay, out var extraModifiers);
			affected.AddRange(extraModifiers);
		}
		decimal total = WatcherFidelityRule.HaltBlock(block, extra);
		await Hook.AfterModifyingBlockAmount(CombatState!, total, this, cardPlay, affected.Distinct());
		await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Unpowered, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(1m);
		DynamicVars["Magic"].UpgradeValueBy(5m);
	}
}
