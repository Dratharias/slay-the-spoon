using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// One shared countdown of 5 cards per turn; each further Panache adds its damage to Amount instead of
/// starting its own countdown, as in StS1 (vanilla's PanachePower is one instance per card).
/// </summary>
public sealed class Sts1PanachePower : PowerModel, IBorrowedIcon
{
	private const int CardsPerTrigger = 5;

	private const string CardsLeftKey = "CardsLeft";

	private class Data
	{
		// Cards whose play began while this power existed: the Panache that applies it doesn't count,
		// a later Panache does.
		public readonly HashSet<CardModel> CountedCards = new();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => DynamicVars[CardsLeftKey].IntValue;

	public Type IconSource => typeof(PanachePower);

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar(CardsLeftKey, CardsPerTrigger),
	};

	protected override object InitInternalData() => new Data();

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == Owner.Player)
		{
			GetInternalData<Data>().CountedCards.Add(cardPlay.Card);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != Owner.Player || !GetInternalData<Data>().CountedCards.Remove(cardPlay.Card))
		{
			return;
		}
		DynamicVars[CardsLeftKey].BaseValue--;
		InvokeDisplayAmountChanged();
		if (DynamicVars[CardsLeftKey].IntValue > 0)
		{
			return;
		}
		DynamicVars[CardsLeftKey].BaseValue = CardsPerTrigger;
		InvokeDisplayAmountChanged();
		if (CombatState != null)
		{
			Flash();
			await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies.ToList(), Amount, ValueProp.Unpowered, Owner, null);
		}
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(Owner))
		{
			DynamicVars[CardsLeftKey].BaseValue = CardsPerTrigger;
			InvokeDisplayAmountChanged();
		}
		return Task.CompletedTask;
	}
}
