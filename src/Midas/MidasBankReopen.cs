using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Context;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Midas;

/// <summary>
/// King Midas: a broken Maw Bank can be reopened at the merchant for gold (owner, 2026-11-23; replaces the rest site
/// option). The bank stays vanilla, so a later purchase breaks it again. The payment is not a merchant purchase: it
/// neither breaks the bank nor raises this merchant's prices.
/// </summary>
public static class MidasBankReopen
{
	public static event Action? Changed;

	/// <summary>Refreshes the merchant buttons (a purchase broke the bank or raised the price).</summary>
	public static void NotifyChanged() => Changed?.Invoke();

	public static bool Broken(Player player) => player.GetRelic<MawBank>() is { HasItemBeenBought: true };

	public static int PriceFor(Player player) =>
		MidasRule.BankReopen(SpoonRoiMidas.Of(player.RunState)?.BoughtHere(player) ?? 0, SpoonRoiMidas.Scale(player.RunState));

	public static bool CanReopen(Player player) => Broken(player) && MidasMelt.CanSpend(player, PriceFor(player));

	public static string Text(Player player)
	{
		var text = new LocString("merchant_room", "SPOON_MIDAS_BANK");
		text.Add("Gold", PriceFor(player));
		return text.GetFormattedText();
	}

	internal static async Task Run(Player player)
	{
		if (!SpoonRoiMidas.IsActive(player.RunState) || !CanReopen(player) || player.GetRelic<MawBank>() is not { } bank) return;
		int price = PriceFor(player);
		await PlayerCmd.LoseGold(price, player, GoldLossType.Spent);
		bank.HasItemBeenBought = false;
		bank.Status = RelicStatus.Normal;
		SpoonLog.Info($"King Midas: player {player.NetId} reopened the Maw Bank for {price} gold");
		Changed?.Invoke();
	}

	public static void Request()
	{
		var run = RunManager.Instance.DebugOnlyGetState();
		if (run == null || LocalContext.GetMe(run) is not { } me) return;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new MidasBankAction(me, new NetMidasBank()));
	}
}

public struct NetMidasBank : INetAction, IPacketSerializable
{
	public GameAction ToGameAction(Player player) => new MidasBankAction(player, this);

	public void Serialize(PacketWriter writer) => writer.WriteBool(true);

	public void Deserialize(PacketReader reader) => reader.ReadBool();

	public override string ToString() => "King Midas bank reopening";
}

public class MidasBankAction(Player player, NetMidasBank action) : GameAction
{
	public override ulong OwnerId => player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	protected override Task ExecuteAction() => MidasBankReopen.Run(player);

	public override INetAction ToNetAction() => action;

	public override string ToString() => $"MidasBankAction for player {player.NetId}";
}
