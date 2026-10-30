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

/// <summary>StS1 Wish: choose one of Plated Armor, Strength or Gold (the options are the token cards; the upgrade upgrades them).</summary>
public sealed class Sts1Wish : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// Plain vars: they only feed the text, and a Damage or Block var would show Strength and Dexterity changes that do not apply.
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Damage", 3m),
		new DynamicVar("Block", 6m),
		new DynamicVar("Magic", 25m),
	};

	public Sts1Wish()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel[] options =
		{
			CombatState!.CreateCard<Sts1BecomeAlmighty>(Owner),
			CombatState!.CreateCard<Sts1FameAndFortune>(Owner),
			CombatState!.CreateCard<Sts1LiveForever>(Owner),
		};
		if (IsUpgraded)
		{
			foreach (CardModel option in options)
			{
				CardCmd.Upgrade(option);
			}
		}
		CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: false);
		switch (chosen)
		{
			case Sts1BecomeAlmighty strength:
				await strength.ApplyChoice(choiceContext, this);
				break;
			case Sts1FameAndFortune gold:
				await gold.ApplyChoice(choiceContext, this);
				break;
			case Sts1LiveForever armor:
				await armor.ApplyChoice(choiceContext, this);
				break;
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Damage"].UpgradeValueBy(1m);
		DynamicVars["Block"].UpgradeValueBy(2m);
		DynamicVars["Magic"].UpgradeValueBy(5m);
	}
}
