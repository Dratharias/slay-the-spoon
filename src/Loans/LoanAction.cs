using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using SlaySpoon.Core;

namespace SlaySpoon.Loans;

public enum LoanActionKind
{
	/// <summary>The sender asks for <see cref="NetLoanAction.Amount"/> gold (0 withdraws the request).</summary>
	Request,
	/// <summary>The sender offers gold to <see cref="NetLoanAction.Borrower"/> (0 withdraws the offer).</summary>
	Offer,
	/// <summary>The sender takes the offers of the players in <see cref="NetLoanAction.Lenders"/>.</summary>
	Accept,
	/// <summary>The sender closes their request without taking anything.</summary>
	Cancel,
}

/// <summary>One step of a merchant loan, sent through the action queue so every client applies it in the same order.</summary>
public struct NetLoanAction : INetAction, IPacketSerializable
{
	private const int MaxAmount = (1 << 16) - 1;

	public LoanActionKind Kind;
	public int Borrower;
	public int Amount;
	public int RatePercent;
	public bool Gift;
	/// <summary>Bit i set: player i's offer is taken.</summary>
	public int Lenders;
	public int Revision;

	public GameAction ToGameAction(Player player)
	{
		return new LoanAction(player, this);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt((int)Kind, 4);
		writer.WriteInt(Math.Clamp(Borrower, 0, 15), 4);
		writer.WriteInt(Math.Clamp(Amount, 0, MaxAmount), 16);
		writer.WriteInt(Math.Clamp(RatePercent, 0, LoanBoard.MaxRate), 10);
		writer.WriteBool(Gift);
		writer.WriteInt(Lenders & 0xFF, 8);
		writer.WriteInt(Revision, 32);
	}

	public void Deserialize(PacketReader reader)
	{
		Kind = (LoanActionKind)reader.ReadInt(4);
		Borrower = reader.ReadInt(4);
		Amount = reader.ReadInt(16);
		RatePercent = reader.ReadInt(10);
		Gift = reader.ReadBool();
		Lenders = reader.ReadInt(8);
		Revision = reader.ReadInt(32);
	}

	public override string ToString()
	{
		return Kind switch
		{
			LoanActionKind.Request => $"asks for {Amount}",
			LoanActionKind.Offer => $"{(Gift ? "gives" : "lends")} {Amount}{(Gift ? "" : $" at {RatePercent}%")} to player {Borrower}",
			LoanActionKind.Accept => $"accepts lenders mask {Lenders}",
			_ => "cancels their request",
		};
	}
}

public class LoanAction : GameAction
{
	private readonly Player _player;
	private readonly NetLoanAction _action;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	public LoanAction(Player player, NetLoanAction action)
	{
		_player = player;
		_action = action;
	}

	protected override Task ExecuteAction()
	{
		MerchantLoans.Apply(_player, _action);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return _action;
	}

	public override string ToString()
	{
		return $"LoanAction for player {_player.NetId}: {_action}";
	}
}
