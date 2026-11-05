using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1ChampionBelt : Sts1Relic
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<WeakPower>() };
	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		// The successful-application hook excludes Artifact-blocked applications and duration reductions.
		if (power is not VulnerablePower || amount <= 0 || applier != Owner.Creature || power.Owner.Side == Owner.Creature.Side) return;
		Flash();
		await PowerCmd.Apply<WeakPower>(choiceContext, power.Owner, 1m, Owner.Creature, cardSource);
	}
}
