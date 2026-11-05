using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1TheSpecimen : Sts1Relic
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<PoisonPower>() };
	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented || Owner.Creature.IsDead || creature.Side == Owner.Creature.Side || creature.CombatState == null) return;
		// Death hooks run before powers are stripped. Each living holder transfers the shared poison once.
		var poison = creature.GetPower<PoisonPower>();
		if (poison == null || poison.Amount <= 0) return;
		var targets = creature.CombatState.GetOpponentsOf(Owner.Creature).Where(c => c != creature && c.IsAlive && c.IsHittable).ToList();
		if (targets.Count == 0) return;
		var target = targets[Owner.RunState.Rng.CombatTargets.NextInt(targets.Count)];
		Flash();
		await PowerCmd.Apply<PoisonPower>(choiceContext, target, poison.Amount, Owner.Creature, null);
	}
}
