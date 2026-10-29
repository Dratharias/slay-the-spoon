using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SlaySpoon.Cards.Sts1.Colorless;
using SlaySpoon.Core;
using SlaySpoon.Relics;

namespace SlaySpoon.Events;

/// <summary>StS1's Augmenter ("Dealer" in French): J.A.X., a 2-card transform, or Mutagenic Strength.</summary>
public sealed class Sts1Augmenter : Sts1Event
{
	private const int Transforms = 2;

	public override Sts1EventKind Kind => Sts1EventKind.Augmenter;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> options = new();
		options.Add(CardUnlocked
			? new EventOption(this, TestJax, "STS1_AUGMENTER.pages.INITIAL.options.JAX", HoverTipFactory.FromCardWithCardHoverTips<Sts1Jax>())
			: new EventOption(this, null, "STS1_AUGMENTER.pages.INITIAL.options.JAX_LOCKED"));
		options.Add(PileType.Deck.GetPile(Owner!).Cards.Count(c => c.IsTransformable) >= Transforms
			? new EventOption(this, BecomeTestSubject, "STS1_AUGMENTER.pages.INITIAL.options.TRANSFORM", HoverTipFactory.Static(StaticHoverTip.Transform))
			: new EventOption(this, null, "STS1_AUGMENTER.pages.INITIAL.options.TRANSFORM_LOCKED"));
		options.Add(new EventOption(this, IngestMutagens, "STS1_AUGMENTER.pages.INITIAL.options.MUTAGENS", HoverTipFactory.FromRelic<Sts1MutagenicStrength>()));
		return options;
	}

	private async Task TestJax()
	{
		CardModel jax = Owner!.RunState.CreateCard<Sts1Jax>(Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(jax, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("STS1_AUGMENTER.pages.JAX.description"));
	}

	private async Task BecomeTestSubject()
	{
		CardSelectorPrefs prefs = new(CardSelectorPrefs.TransformSelectionPrompt, Transforms);
		List<CardModel> cards = (await CardSelectCmd.FromDeckForTransformation(Owner!, prefs)).ToList();
		foreach (CardModel card in cards)
		{
			await CardCmd.TransformToRandom(card, Rng, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("STS1_AUGMENTER.pages.TRANSFORM.description"));
	}

	private async Task IngestMutagens()
	{
		await RelicCmd.Obtain<Sts1MutagenicStrength>(Owner!);
		SetEventFinished(L10NLookup("STS1_AUGMENTER.pages.MUTAGENS.description"));
	}
}
