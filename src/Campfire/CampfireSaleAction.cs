using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace SlaySpoon.Campfire;

public enum CampfireSaleKind { Offer, Accept, Decline, Cancel }

/// <summary>Offers and replies use the shared action queue, never the buyer's concurrent rest-site choice IDs.</summary>
public struct NetCampfireSaleAction : INetAction, IPacketSerializable
{
	public CampfireSaleKind Kind;
	public int Seller, Buyer, Card, Price, Rate, Cash;
	public uint SaleId;

	public GameAction ToGameAction(Player player) => new CampfireSaleAction(player, this);

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)Kind);
		writer.WriteInt(Seller);
		writer.WriteInt(Buyer);
		writer.WriteInt(Card);
		writer.WriteInt(Price);
		writer.WriteInt(Rate);
		writer.WriteInt(Cash);
		writer.WriteInt(unchecked((int)SaleId));
	}

	public void Deserialize(PacketReader reader)
	{
		Kind = (CampfireSaleKind)reader.ReadInt();
		Seller = reader.ReadInt();
		Buyer = reader.ReadInt();
		Card = reader.ReadInt();
		Price = reader.ReadInt();
		Rate = reader.ReadInt();
		Cash = reader.ReadInt();
		SaleId = unchecked((uint)reader.ReadInt());
	}
}

public sealed class CampfireSaleAction(Player player, NetCampfireSaleAction action) : GameAction
{
	public override ulong OwnerId => player.NetId;
	public override GameActionType ActionType => GameActionType.NonCombat;
	protected override Task ExecuteAction() => CampfireSales.Apply(player, action);
	public override INetAction ToNetAction() => action;
}
