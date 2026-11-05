using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.CardSets;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;
using SlaySpoon.Watcher;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Wish: choose one of Plated Armor, Strength or Gold (the options are the token cards; the upgrade upgrades them).</summary>
public sealed class Sts1Wish : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// Plain vars, not Damage or Block: they only feed the text, and DynamicVarSet.Damage and .Block hard-cast to DamageVar and BlockVar.
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Strength", 3m),
		new DynamicVar("PlatedArmor", 6m),
		new DynamicVar("Gold", 25m),
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
		DynamicVars["Strength"].UpgradeValueBy(1m);
		DynamicVars["PlatedArmor"].UpgradeValueBy(2m);
		DynamicVars["Gold"].UpgradeValueBy(5m);
	}
}
