using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1HoveringKite : Sts1InactiveBossRelic
{
	private bool _triggered;
	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
	 if (participants.Contains(Owner.Creature)) _triggered = false;
	 return Task.CompletedTask;
	}
	public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
	{
	 if (_triggered || card.Owner != Owner || Owner.Creature.CombatState.CurrentSide != Owner.Creature.Side) return;
	 _triggered = true;
	 Flash();
	 await PlayerCmd.GainEnergy(1m, Owner);
	}
}
