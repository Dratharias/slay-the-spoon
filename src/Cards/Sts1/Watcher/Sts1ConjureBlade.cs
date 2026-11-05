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
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Conjure Blade: X cost. Shuffle an Expunger that hits X (X+1 upgraded) times into the owner's draw pile.</summary>
public sealed class Sts1ConjureBlade : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<Sts1Expunger>() };

	protected override bool HasEnergyCostX => true;

	public Sts1ConjureBlade()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Sts1Expunger blade = CombatState!.CreateCard<Sts1Expunger>(Owner);
		// ResolveEnergyXValue already includes Chemical X.
		blade.SetHits(ResolveEnergyXValue() + (IsUpgraded ? 1 : 0));
		await CardPileCmd.AddGeneratedCardToCombat(blade, PileType.Draw, Owner, CardPilePosition.Random);
	}
}
