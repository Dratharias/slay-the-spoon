using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Blasphemer (from Blasphemy): the owner dies at the start of their next turn. Fairy in a Bottle can still save them.</summary>
public sealed class Sts1EndTurnDeathPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(DoomPower);

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner) || Owner.IsDead)
		{
			return;
		}
		Flash();
		await PowerCmd.Remove(this);
		await CreatureCmd.Kill(Owner);
	}
}
