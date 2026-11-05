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

public sealed class Sts1Inserter : Sts1BossRelic
{
	private int _turnsSeen;
	[SavedProperty]
	public int TurnsSeen
	{
	 get => _turnsSeen;
	 set { AssertMutable(); _turnsSeen = value; InvokeDisplayAmountChanged(); }
	}
	public override bool ShowCounter => true;
	public override int DisplayAmount => TurnsSeen;
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
	 if (!participants.Contains(Owner.Creature)) return;
	 TurnsSeen = Sts1RelicRule.InserterCounter(TurnsSeen);
	 if (TurnsSeen != 0) return;
	 Flash();
	 await OrbCmd.AddSlots(Owner, 1);
	}
}
