using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Spirit Shield: Block for each other card in the owner's hand.</summary>
public sealed class Sts1SpiritShield : Sts1WatcherCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 3m),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		// Every other card in the hand: the played card has left it by OnPlay, and is skipped while it is previewed there.
		new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => WatcherSkillMath.SpiritShieldBlock(
			PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card), card.DynamicVars["Magic"].IntValue)),
	};

	public Sts1SpiritShield()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.Calculate(cardPlay.Target),
			DynamicVars.CalculatedBlock.Props, cardPlay);
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}
}
