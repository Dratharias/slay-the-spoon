using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Collect: X cost. For the next X (X+1 upgraded) turns, an upgraded Miracle goes into the owner's hand at turn start.</summary>
public sealed class Sts1Collect : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<Sts1Miracle>(true) };

	protected override bool HasEnergyCostX => true;

	public Sts1Collect()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// X is the energy spent; the game already adds Chemical X's bonus to it.
		int turns = WatcherSkillMath.CollectTurns(ResolveEnergyXValue(), IsUpgraded);
		if (turns > 0)
		{
			await PowerCmd.Apply<Sts1CollectPower>(choiceContext, Owner.Creature, turns, Owner.Creature, this);
		}
	}
}
