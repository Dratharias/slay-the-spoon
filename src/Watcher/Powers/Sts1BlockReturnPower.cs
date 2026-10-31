using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1 Block Return, put on an enemy by Talk to the Hand: whoever attacks this enemy gains Block. In co-op the Block goes
/// to the attacking player, not to the player who applied it.
/// </summary>
public sealed class Sts1BlockReturnPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(ThornsPower);

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
		DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		// AfterDamageGiven includes lethal hits, before the target's powers are removed.
		// A pet attack belongs to its player: StS1 has no pets, and always blocks the player.
		Creature? recipient = (dealer?.Player ?? dealer?.PetOwner)?.Creature;
		if (target != Owner || recipient == null || dealer == Owner || !props.IsPoweredAttack())
		{
			return;
		}
		Flash();
		await CreatureCmd.GainBlock(recipient, Amount, ValueProp.Unpowered, null);
	}
}
