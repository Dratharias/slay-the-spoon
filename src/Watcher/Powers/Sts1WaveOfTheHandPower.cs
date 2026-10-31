using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Wave of the Hand: this turn, each time the owner gains Block, ALL enemies get Weak.</summary>
public sealed class Sts1WaveOfTheHandPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(WeakPower);

	public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (creature != Owner || amount <= 0m)
		{
			return;
		}
		Flash();
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), CombatState.HittableEnemies.ToList(), Amount, Owner, null);
	}

	public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		// Keep enemy end-of-turn Block hooks active before removing the round-long effect.
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.Remove(this);
		}
	}
}
