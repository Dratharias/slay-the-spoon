using MegaCrit.Sts2.Core.CardSelection;
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
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Watcher;

namespace SlaySpoon.Cards.Sts1.Watcher;

/// <summary>StS1 Omniscience: choose a card in the draw pile and play it Magic times, exhausting it.</summary>
public sealed class Sts1Omniscience : Sts1WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Magic", 2m),
	};

	public Sts1Omniscience()
		: base(4, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPile draw = PileType.Draw.GetPile(Owner);
		if (draw.Cards.Count == 0)
		{
			return;
		}
		CardModel? chosen = (await CardSelectCmd.FromSimpleGrid(choiceContext, draw.Cards.ToList(), Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
		if (chosen == null)
		{
			return;
		}
		// StS1 queues the chosen card plus stat-equivalent copies; make the copies first so all share the card's current state.
		var copies = new List<CardModel>();
		for (int i = 1; i < DynamicVars["Magic"].IntValue; i++)
		{
			copies.Add(CombatState!.CloneCard(chosen));
		}
		await CardPileCmd.Add(chosen, PileType.Play);
		chosen.ExhaustOnNextPlay = true;
		await CardCmd.AutoPlay(choiceContext, chosen, null);
		foreach (CardModel copy in copies)
		{
			using (OmnisciencePurge.Mark(copy))
				await CardCmd.AutoPlay(choiceContext, copy, null);
		}
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
