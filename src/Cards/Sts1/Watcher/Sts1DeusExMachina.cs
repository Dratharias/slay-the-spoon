using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Deus Ex Machina: unplayable. When its owner draws it, Magic Miracles go into their hand and it exhausts.</summary>
public sealed class Sts1DeusExMachina : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, CardKeyword.Unplayable };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<Sts1Miracle>() };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 2m),
	};

	public Sts1DeusExMachina()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// Unplayable: the effect happens when drawn.
		await Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Magic"].UpgradeValueBy(1m);
	}


	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState == null)
		{
			return;
		}
		var miracles = new List<CardModel>();
		for (int i = 0; i < DynamicVars["Magic"].IntValue; i++)
		{
			miracles.Add(CombatState!.CreateCard<Sts1Miracle>(Owner));
		}
		await CardPileCmd.AddGeneratedCardsToCombat(miracles, PileType.Hand, Owner);
		await CardCmd.Exhaust(choiceContext, this);
	}
}
