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

public sealed class Sts1RingOfTheSerpent : Sts1BossRelic
{
	public override bool HasUponPickupEffect => true;
	public override async Task AfterObtained()
	{
	 var starter = Owner.Relics.OfType<RingOfTheSnake>().FirstOrDefault();
	 if (starter != null) await RelicCmd.Remove(starter);
	}
	public override decimal ModifyHandDraw(Player player, decimal count) => player == Owner ? count + 1 : count;
}
