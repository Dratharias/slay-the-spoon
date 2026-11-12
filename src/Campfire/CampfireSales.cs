using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Loans;

namespace SlaySpoon.Campfire;

public sealed record CampfireSale(Player Seller, Player Buyer, CardModel Card, uint Id,
	int Price, int Rate, int PaidNow, int Debt);

/// <summary>One accepted transaction moves a card, its cash payment and any debt, in the shared action order.</summary>
public static class CampfireSales
{
	private sealed class Pending
	{
		public readonly TaskCompletionSource<bool> Result = new();
		public CampfireSale? Offer;
		public CampfireSalePanel? Panel;
		public bool Resolving;
	}

	private static IRunState? _run;
	private static readonly Dictionary<(ulong Seller, uint Id), Pending> PendingSales = new();

	private static Pending Get(Player seller, uint id)
	{
		if (_run != seller.RunState) { Close(); _run = seller.RunState; }
		var key = (seller.NetId, id);
		if (!PendingSales.TryGetValue(key, out var pending)) PendingSales[key] = pending = new Pending();
		return pending;
	}

	public static Task<bool> WaitForResult(Player seller, uint id) => Get(seller, id).Result.Task;

	public static void Send(Player sender, NetCampfireSaleAction action) =>
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new CampfireSaleAction(sender, action));

	public static async Task Apply(Player sender, NetCampfireSaleAction action)
	{
		var run = sender.RunState;
		if (run.CurrentRoom is not RestSiteRoom || !run.Modifiers.OfType<SpoonCampfireGift>().Any()
			|| MerchantLoans.PlayerAt(run, action.Seller) is not { } seller) return;
		var pending = Get(seller, action.SaleId);
		if (pending.Result.Task.IsCompleted || pending.Resolving) return;
		switch (action.Kind)
		{
			case CampfireSaleKind.Offer:
				if (sender != seller || pending.Offer != null) return;
				var buyer = MerchantLoans.PlayerAt(run, action.Buyer);
				var deck = PileType.Deck.GetPile(seller).Cards;
				if (buyer == null || !CampfireGiftRule.CanReceive(seller == buyer, !buyer.Creature.IsDead)
					|| seller.Creature.IsDead || !CampfireGiftRule.ValidTerms(action.Price, action.Rate)
					|| action.Cash < 0 || action.Cash > action.Price
					|| action.Card < 0 || action.Card >= deck.Count
					|| !CampfireGiftRule.CanGift(deck[action.Card].Type.ToString(), deck[action.Card].IsRemovable))
				{
					Fail(pending, "failed", seller, buyer);
					return;
				}
				var payment = CampfireGiftRule.Payment(action.Price, action.Rate, action.Cash);
				pending.Offer = new CampfireSale(seller, buyer, deck[action.Card], action.SaleId,
					action.Price, action.Rate, payment.PaidNow, payment.Debt);
				if (RunManager.Instance.NetService.Type != NetGameType.Replay
					&& (LocalContext.IsMe(seller) || LocalContext.IsMe(buyer)))
				{
					pending.Panel = CampfireSalePanel.ShowOffer(pending.Offer);
					// A buyer who cannot see the offer declines it, so the seller's rest action never waits forever.
					if (pending.Panel == null && LocalContext.IsMe(buyer))
					{
						var declined = new NetCampfireSaleAction { Kind = CampfireSaleKind.Decline,
							Seller = action.Seller, SaleId = action.SaleId };
						Callable.From(() => Send(buyer, declined)).CallDeferred();
					}
				}
				break;
			case CampfireSaleKind.Cancel:
				if (sender == seller) Finish(pending, false);
				break;
			case CampfireSaleKind.Decline:
				if (pending.Offer?.Buyer != sender) return;
				Finish(pending, false);
				if (LocalContext.IsMe(seller) && RunManager.Instance.NetService.Type != NetGameType.Replay)
					CampfireSalePanel.ShowNotice("declined");
				break;
			case CampfireSaleKind.Accept:
				if (pending.Offer is not { } sale || sale.Buyer != sender) return;
				// Honor exactly the displayed payment. Never silently increase the debt if the buyer spent gold.
				if (seller.Creature.IsDead || sender.Creature.IsDead || sender.Gold < sale.PaidNow
					|| sale.Card.Owner != seller || sale.Card.Pile?.Type != PileType.Deck
					|| !CampfireGiftRule.CanGift(sale.Card.Type.ToString(), sale.Card.IsRemovable)
					|| MerchantLoans.LedgerOf(run) is not { } ledger)
				{
					Fail(pending, sender.Gold < sale.PaidNow ? "failed_gold" : "failed", seller, sender);
					return;
				}
				pending.Resolving = true;
				pending.Panel?.Dismiss();
				bool sold = false;
				try
				{
					// The card moves first: if a deck command throws, no gold or debt has changed hands.
					var copy = CardModel.FromSerializable(sale.Card.ToSerializable());
					await CardPileCmd.RemoveFromDeck(sale.Card);
					run.AddCard(copy, sender);
					await CardPileCmd.Add(copy, PileType.Deck);
					// Plain gold transfers match merchant loans: neither relics nor debt hooks alter a sale's price.
					sender.Gold -= sale.PaidNow;
					seller.Gold += sale.PaidNow;
					ledger.AddLoan(MerchantLoans.IndexOf(sender), [(action.Seller, sale.Debt)]);
					MerchantLoans.LedgerChanged(ledger, $"campfire sale {sale.Price}, paid {sale.PaidNow}, debt {sale.Debt}");
					SpoonLog.Info($"campfire: {seller.NetId} sold {copy.Id.Entry} to {sender.NetId} for {sale.Price}, debt {sale.Debt}");
					sold = true;
				}
				finally
				{
					// Always release the seller's rest action, even when a deck command throws.
					Finish(pending, sold);
				}
				break;
		}
	}

	/// <summary>Ends the sale and tells both players why it fell through.</summary>
	private static void Fail(Pending pending, string reason, Player seller, Player? buyer)
	{
		Finish(pending, false);
		SpoonLog.Info($"campfire: sale by {seller.NetId} failed ({reason})");
		if (RunManager.Instance.NetService.Type != NetGameType.Replay
			&& (LocalContext.IsMe(seller) || (buyer != null && LocalContext.IsMe(buyer))))
			CampfireSalePanel.ShowNotice(reason);
	}

	private static void Finish(Pending pending, bool sold)
	{
		pending.Panel?.Dismiss();
		pending.Panel = null;
		pending.Result.TrySetResult(sold);
	}

	public static void Close()
	{
		foreach (var pending in PendingSales.Values.ToArray()) Finish(pending, false);
		PendingSales.Clear();
		_run = null;
	}
}

[HarmonyPatch(typeof(RunManager), "ExitCurrentRooms")]
internal static class CampfireSalesRoomExitPatch
{
	private static void Prefix() => CampfireSales.Close();
}
