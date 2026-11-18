using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>Whenever its owner plays a card, deal Amount unpowered damage to ALL enemies.</summary>
public sealed class Sts1AThousandCutsPower : PowerModel, IBorrowedIcon
{
	private class Data
	{
		// Amount when each play started: the card that applies the power doesn't trigger it, and a second
		// copy deals the old amount before stacking. Keyed by play, not card, so a card played again inside
		// its own play (each play is a new CardPlay) gets both hits.
		public readonly Dictionary<CardPlay, int> AmountsForPlays = new();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(PhantomBladesPower);

	protected override object InitInternalData() => new Data();

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == Owner)
		{
			GetInternalData<Data>().AmountsForPlays[cardPlay] = Amount;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == Owner
			&& GetInternalData<Data>().AmountsForPlays.Remove(cardPlay, out int amount)
			&& amount > 0 && CombatState != null)
		{
			Flash();
			await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies.ToList(), amount, ValueProp.Unpowered, Owner, null, null);
		}
	}
}
