using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

public sealed class Sts1HeatsinksPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(MachineLearningPower);

	// Capture the amount before the card applies its power: Heatsinks does not trigger itself.
	private sealed class PendingDraws
	{
		internal readonly Dictionary<CardPlay, int> Amounts = new();
	}

	protected override object InitInternalData() => new PendingDraws();

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Power)
		{
			GetInternalData<PendingDraws>().Amounts[cardPlay] = Amount;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (GetInternalData<PendingDraws>().Amounts.Remove(cardPlay, out int cards))
		{
			Flash();
			await CardPileCmd.Draw(choiceContext, cards, Owner.Player!);
		}
	}
}
