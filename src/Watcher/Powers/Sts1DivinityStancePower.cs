using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1's Divinity stance: the holder's card attacks deal triple damage (entering gives energy, through
/// <see cref="StanceCmd"/>). The holder returns to Neutral at the start of their next turn, before drawing, as in StS1,
/// where the stance ends with the start-of-turn relics, ahead of the draw.
/// </summary>
public sealed class Sts1DivinityStancePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(RadiancePower);

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer == Owner && cardSource != null && props.IsPoweredAttack())
		{
			return StanceMath.DealtMultiplier(Stance.Divinity);
		}
		return 1m;
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == Owner.Player)
		{
			await StanceCmd.Enter(player, Stance.Neutral, null, choiceContext);
		}
	}
}
