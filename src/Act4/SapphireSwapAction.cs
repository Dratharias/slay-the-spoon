using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace SlaySpoon.Act4;

public struct NetSapphireSwap : INetAction, IPacketSerializable
{
	public int RelicIndex;
	public int Floor;
	public GameAction ToGameAction(Player player) => new SapphireSwapAction(player, this);
	public void Serialize(PacketWriter writer) { writer.WriteInt(RelicIndex, 16); writer.WriteInt(Floor, 16); }
	public void Deserialize(PacketReader reader) { RelicIndex = reader.ReadInt(16); Floor = reader.ReadInt(16); }
}

/// <summary>The action queue decides which player gives up their relic when two players accept.</summary>
public class SapphireSwapAction(Player player, NetSapphireSwap action) : GameAction
{
	public override ulong OwnerId => player.NetId;
	public override GameActionType ActionType => GameActionType.NonCombat;
	protected override Task ExecuteAction() => SapphireChestPatch.Swap(player, action);
	public override INetAction ToNetAction() => action;
}
