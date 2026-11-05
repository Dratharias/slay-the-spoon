using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1 Block Return, put on an enemy by Talk to the Hand: whoever attacks this enemy gains Block. In co-op the Block goes
/// to the attacking player, not to the player who applied it.
/// </summary>
/// <remarks>Its StS1 icon (talk_to_hand) is listed in <see cref="Core.Sts1PowerIcons"/>.</remarks>
public sealed class Sts1BlockReturnPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

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
