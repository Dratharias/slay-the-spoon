using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Core;

namespace SlaySpoon.Achilles;

/// <summary>
/// Achilles' Heel: unblocked powered attack damage to the owner kills them (<see cref="AchillesRule.HeelKills"/>).
/// Same trigger as The Gambit, but never removed, so the heel stays after a revive. A Buff, so Artifact ignores it.
/// </summary>
public sealed class SpoonHeelPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(TheGambitPower);

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != Owner || !AchillesRule.HeelKills(props.IsPoweredAttack(), result.UnblockedDamage)) return;
		Flash();
		await CreatureCmd.Kill(Owner);
	}
}
