using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>When its owner dies, deal Amount times its Max HP to every other enemy (unpowered, like StS1's thorns damage).</summary>
public sealed class Sts1CorpseExplosionPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(MagicBombPower);

	// The game raises this hook before it strips the dead creature's powers.
	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented || creature != Owner || Owner.CombatState == null)
		{
			return;
		}
		List<Creature> targets = Owner.CombatState.HittableEnemies.Where(e => e != Owner).ToList();
		if (targets.Count == 0)
		{
			return;
		}
		Flash();
		await CreatureCmd.Damage(choiceContext, targets, Owner.MaxHp * Amount, ValueProp.Unpowered, Applier, null);
	}
}
