using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Ui;

namespace SlaySpoon.Midas;

/// <summary>
/// King Midas relic sale (spec docs/superpowers/specs/2026-11-23-midas-relic-sale-design.md): the merchant's Sell button
/// queues an action; every client runs the same synced relic choice, removes the relic and pays 75 % of its merchant
/// price. The seller's own shop then offers the relic back (<see cref="MidasResale"/>).
/// </summary>
public static class MidasRelicSale
{
	private static readonly HashSet<ModelId> StartPack = MidasStart.Relics().Select(r => r.Id).ToHashSet();

	private static MidasRelicRarity RarityOf(RelicModel relic) =>
		Enum.TryParse(relic.Rarity.ToString(), out MidasRelicRarity rarity) ? rarity : MidasRelicRarity.None;

	public static bool CanSell(RelicModel relic) => MidasRule.CanSell(RarityOf(relic), StartPack.Contains(relic.Id));

	public static IReadOnlyList<RelicModel> Sellable(Player player) => player.Relics.Where(CanSell).ToList();

	public static int PriceOf(RelicModel relic) => MidasRule.SalePrice(relic.MerchantCost);

	public static event Action? Changed;

	internal static async Task Run(Player player)
	{
		if (!SpoonRoiMidas.IsActive(player.RunState)) return;
		var sellable = Sellable(player);
		if (sellable.Count == 0) return;
		RelicModel? relic = await RelicSelectCmd.FromChooseARelicScreen(player, sellable);
		if (relic == null || !player.Relics.Contains(relic)) return;
		int price = PriceOf(relic);
		await RelicCmd.Remove(relic);
		await PlayerCmd.GainGold(price, player);
		SpoonLog.Info($"King Midas: player {player.NetId} sold {relic.Id.Entry} for {price} gold");
		if (LocalContext.IsMe(player)) MidasResale.Offer(relic, player, price);
		Changed?.Invoke();
	}

	public static void Request()
	{
		var run = RunManager.Instance.DebugOnlyGetState();
		if (run == null || LocalContext.GetMe(run) is not { } me) return;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new MidasSaleAction(me, new NetMidasSale()));
	}
}

public struct NetMidasSale : INetAction, IPacketSerializable
{
	public GameAction ToGameAction(Player player) => new MidasSaleAction(player, this);

	public void Serialize(PacketWriter writer) => writer.WriteBool(true);

	public void Deserialize(PacketReader reader) => reader.ReadBool();

	public override string ToString() => "King Midas relic sale";
}

public class MidasSaleAction(Player player, NetMidasSale action) : GameAction
{
	public override ulong OwnerId => player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	protected override Task ExecuteAction() => MidasRelicSale.Run(player);

	public override INetAction ToNetAction() => action;

	public override string ToString() => $"MidasSaleAction for player {player.NetId}";
}

/// <summary>
/// A sold relic back on sale in the seller's shop, local only (merchant inventories are per player and vanilla syncs a
/// purchase by its result). Vanilla would price a new relic entry with <c>PlayerRng.Shops</c> and refill a bought slot
/// from the relic grab bag; both are skipped so the run's seed stays vanilla.
/// </summary>
public static class MidasResale
{
	private sealed class Floor(int value)
	{
		public readonly int Value = value;
	}

	private static readonly ConditionalWeakTable<MerchantEntry, Floor> Floors = new();

	[ThreadStatic] private static bool _creating;

	private static readonly AccessTools.FieldRef<MerchantEntry, int> CostField = AccessTools.FieldRefAccess<MerchantEntry, int>("_cost");

	public static event Action<MerchantRelicEntry>? Offered;

	internal static bool IsResale(MerchantEntry entry) => _creating || Floors.TryGetValue(entry, out _);

	internal static int? FloorOf(MerchantEntry entry) => Floors.TryGetValue(entry, out var floor) ? floor.Value : null;

	internal static void SetBaseCost(MerchantRelicEntry entry) => CostField(entry) = entry.Model?.MerchantCost ?? 0;

	internal static void Offer(RelicModel sold, Player player, int salePrice)
	{
		MerchantRelicEntry entry;
		_creating = true;
		try { entry = new MerchantRelicEntry(sold.CanonicalInstance.ToMutable(), player); }
		finally { _creating = false; }
		Floors.Add(entry, new Floor(MidasRule.ResaleFloor(salePrice)));
		try { Offered?.Invoke(entry); }
		catch (Exception e) { SpoonLog.Error($"King Midas: resale slot failed: {e.Message}"); }
	}
}

[HarmonyPatch(typeof(MerchantRelicEntry), nameof(MerchantRelicEntry.CalcCost))]
public static class MidasResaleCostPatch
{
	public static bool Prefix(MerchantRelicEntry __instance)
	{
		if (!MidasResale.IsResale(__instance)) return true;
		MidasResale.SetBaseCost(__instance);
		return false;
	}
}

/// <summary>A bought resale slot empties instead of pulling a new relic from the grab bag.</summary>
[HarmonyPatch(typeof(MerchantRelicEntry), "RestockAfterPurchase")]
public static class MidasResaleRestockPatch
{
	private static readonly Action<MerchantRelicEntry> Clear =
		AccessTools.MethodDelegate<Action<MerchantRelicEntry>>(AccessTools.Method(typeof(MerchantRelicEntry), "ClearAfterPurchase"));

	public static bool Prefix(MerchantRelicEntry __instance)
	{
		if (!MidasResale.IsResale(__instance)) return true;
		Clear(__instance);
		return false;
	}
}

/// <summary>Discounts (Membership Card, The Courier) never bring the buy-back under the sale price plus a quarter.</summary>
[HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.Cost), MethodType.Getter)]
public static class MidasResaleFloorPatch
{
	public static void Postfix(MerchantEntry __instance, ref int __result)
	{
		if (MidasResale.FloorOf(__instance) is { } floor && __result < floor) __result = floor;
	}
}

/// <summary>The merchant room's King Midas buttons: Sell a relic, Reopen the Maw Bank, and the resale slots.</summary>
[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._Ready))]
public static class MidasMerchantButtonsPatch
{
	private const float Step = 70;

	public static void Postfix(NMerchantRoom __instance)
	{
		var run = RunManager.Instance.DebugOnlyGetState();
		if (!SpoonRoiMidas.IsActive(run) || LocalContext.GetMe(run!) is not { } me) return;
		var parent = __instance.Inventory;
		float y = 130 + (SpoonForge.Of(run) != null ? Step : 0);
		var sell = NativeTradePopup.Button(parent, new LocString("merchant_room", "SPOON_MIDAS_SELL").GetFormattedText(),
			MidasRelicSale.Request, disabled: MidasRelicSale.Sellable(me).Count == 0);
		Place(sell, new Vector2(-340, y));
		var bank = NativeTradePopup.Button(parent, MidasBankReopen.Text(me), MidasBankReopen.Request, disabled: !MidasBankReopen.CanReopen(me));
		Place(bank, new Vector2(-340, y + Step));
		int slots = 0;
		void Refresh()
		{
			if (GodotObject.IsInstanceValid(sell))
			{
				if (MidasRelicSale.Sellable(me).Count > 0) sell.Enable();
				else sell.Disable();
			}
			if (GodotObject.IsInstanceValid(bank))
			{
				bank.Visible = MidasBankReopen.Broken(me);
				bank.SetText(MidasBankReopen.Text(me));
				if (MidasBankReopen.CanReopen(me)) bank.Enable();
				else bank.Disable();
			}
		}
		void Offered(MerchantRelicEntry entry)
		{
			if (!GodotObject.IsInstanceValid(parent)) return;
			var template = parent.GetNodeOrNull<Control>("%Relics")?.GetChildren().OfType<NMerchantRelic>().FirstOrDefault();
			if (template?.Duplicate() is not NMerchantRelic slot) return;
			parent.AddChild(slot);
			// The copy carries the template's relic icon; FillSlot adds the sold relic's own.
			foreach (var old in slot.GetNode<Control>("%RelicHolder").GetChildren()) old.QueueFree();
			slot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
			slot.Position += new Vector2(-340 + 130 * slots++, y + 2 * Step + 40);
			slot.Initialize(parent);
			slot.FillSlot(entry);
		}
		Refresh();
		MidasRelicSale.Changed += Refresh;
		MidasBankReopen.Changed += Refresh;
		MidasResale.Offered += Offered;
		me.GoldChanged += Refresh;
		__instance.TreeExiting += () =>
		{
			MidasRelicSale.Changed -= Refresh;
			MidasBankReopen.Changed -= Refresh;
			MidasResale.Offered -= Offered;
			me.GoldChanged -= Refresh;
		};
	}

	private static void Place(Control button, Vector2 offset)
	{
		button.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
		button.Position += offset;
	}
}
