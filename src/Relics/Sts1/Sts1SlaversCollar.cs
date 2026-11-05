using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1SlaversCollar : Sts1EnergyBossRelic
{
	protected override bool GivesEnergy(Player player) => CombatManager.Instance.IsInProgress
		&& player.RunState.CurrentRoom?.RoomType is RoomType.Elite or RoomType.Boss;
}
