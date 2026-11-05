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

public sealed class Sts1FrozenCore : Sts1InactiveBossRelic
{
	public override bool HasUponPickupEffect => true;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromOrb<FrostOrb>() };
	public override async Task AfterObtained()
	{
	 var starter = Owner.Relics.OfType<CrackedCore>().FirstOrDefault();
	 if (starter != null) await RelicCmd.Remove(starter);
	}
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
	 if (!participants.Contains(Owner.Creature)) return;
	 var queue = Owner.PlayerCombatState.OrbQueue;
	 if (queue.Orbs.Count >= queue.Capacity) return;
	 Flash();
	 await OrbCmd.Channel<FrostOrb>(choiceContext, Owner);
	}
}
