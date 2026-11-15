using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Folie;

/// <summary>One locked bid: the chest it belongs to (a hash of the chest's key) and the stake index (-1: none).</summary>
public struct FolieBidMessage : INetMessage
{
	public uint Chest;
	public int Bid;

	public bool ShouldBroadcast => true;
	public NetTransferMode Mode => NetTransferMode.Reliable;
	public LogLevel LogLevel => LogLevel.Debug;
	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteUInt(Chest);
		writer.WriteInt(Bid);
	}

	public void Deserialize(PacketReader reader)
	{
		Chest = reader.ReadUInt();
		Bid = reader.ReadInt();
	}
}

/// <summary>
/// Bids travel as their own message keyed by the chest, not through the bidders' player-choice ids: a chest opened by
/// one player's event must not take an id the other players are using for their own screens, in whatever order
/// their clients reserve them. Bids that arrive before the chest opens locally are kept until it asks.
/// </summary>
public static class FolieBids
{
	private static readonly Dictionary<(uint Chest, ulong Player), TaskCompletionSource<int?>> Bids = new();
	private static INetGameService? _service;

	internal static void Attach(INetGameService service)
	{
		Detach();
		Bids.Clear();
		_service = service;
		service.RegisterMessageHandler<FolieBidMessage>(OnBid);
	}

	internal static void Detach()
	{
		_service?.UnregisterMessageHandler<FolieBidMessage>(OnBid);
		_service = null;
	}

	/// <summary>A stable key hash (FNV-1a), the same on every client.</summary>
	public static uint Hash(string key)
	{
		uint hash = 2166136261;
		foreach (char c in key)
		{
			hash ^= c;
			hash *= 16777619;
		}
		return hash;
	}

	public static void SendLocal(uint chest, Player player, int? bid)
	{
		Slot(chest, player.NetId).TrySetResult(bid);
		_service?.SendMessage(new FolieBidMessage { Chest = chest, Bid = bid ?? -1 });
	}

	public static Task<int?> WaitFor(uint chest, Player player) => Slot(chest, player.NetId).Task;

	/// <summary>Forgets a resolved chest's bids.</summary>
	public static void Forget(uint chest, IEnumerable<Player> players)
	{
		foreach (Player player in players) Bids.Remove((chest, player.NetId));
	}

	private static void OnBid(FolieBidMessage message, ulong sender) =>
		Slot(message.Chest, sender).TrySetResult(message.Bid < 0 ? null : message.Bid);

	private static TaskCompletionSource<int?> Slot(uint chest, ulong player)
	{
		if (!Bids.TryGetValue((chest, player), out var slot))
		{
			slot = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
			Bids[(chest, player)] = slot;
		}
		return slot;
	}
}

/// <summary>Each run's (or loaded run's) synchronizers come with a fresh bid table on the same network service.</summary>
[HarmonyPatch(typeof(PlayerChoiceSynchronizer), MethodType.Constructor, typeof(INetGameService), typeof(IPlayerCollection))]
internal static class FolieBidsAttachPatch
{
	private static void Postfix(INetGameService netService)
	{
		try
		{
			FolieBids.Attach(netService);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: bid handler not registered: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(PlayerChoiceSynchronizer), nameof(PlayerChoiceSynchronizer.Dispose))]
internal static class FolieBidsDetachPatch
{
	private static void Postfix() => FolieBids.Detach();
}
