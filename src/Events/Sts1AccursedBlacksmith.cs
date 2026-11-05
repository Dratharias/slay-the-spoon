using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards.Sts1.Curses;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Ominous Forge (Accursed Blacksmith): upgrade a deck card (locked when none can be), rummage for Warped Tongs
/// and the Pain curse, or leave.
/// </summary>
public sealed class Sts1AccursedBlacksmith : Sts1Event
{
	private const string Key = "STS1_ACCURSED_BLACKSMITH.pages.";

	public override Sts1EventKind Kind => Sts1EventKind.AccursedBlacksmith;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		PileType.Deck.GetPile(Owner!).Cards.Any(c => c?.IsUpgradable ?? false)
			? new(this, Forge, Key + "INITIAL.options.FORGE")
			: new(this, null, Key + "INITIAL.options.FORGE_LOCKED"),
		new(this, Rummage, Key + "INITIAL.options.RUMMAGE",
			HoverTipFactory.FromRelic<Sts1WarpedTongs>().Concat(HoverTipFactory.FromCardWithCardHoverTips<Sts1Pain>())),
		new(this, Leave, Key + "INITIAL.options.LEAVE"),
	};

	private async Task Forge()
	{
		CardModel? card = (await CardSelectCmd.FromDeckForUpgrade(Owner!,
			new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
		if (card != null) CardCmd.Upgrade(card);
		SetEventFinished(L10NLookup(Key + "FORGE.description"));
	}

	private async Task Rummage()
	{
		await RelicCmd.Obtain(ModelDb.Relic<Sts1WarpedTongs>().ToMutable(), Owner!);
		await CardPileCmd.AddCurseToDeck<Sts1Pain>(Owner!);
		SetEventFinished(L10NLookup(Key + "RUMMAGE.description"));
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup(Key + "LEAVE.description"));
		return Task.CompletedTask;
	}
}
