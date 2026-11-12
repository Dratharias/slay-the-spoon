using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using SlaySpoon.Core;
using SlaySpoon.Loans;
using SlaySpoon.Ui;

namespace SlaySpoon.Campfire;

/// <summary>Native popup and card preview for pricing, acceptance and cancellation.</summary>
public sealed class CampfireSalePanel
{
	private readonly NativeTradePopup _popup;
	private readonly TaskCompletionSource<(int Price, int Rate)?> _terms = new();
	private CampfireSale? _offer;
	private int _price, _rate;
	private bool _sent;

	private static string Text(string key) => new LocString("rest_site_ui", "SPOON_SALE." + key).GetFormattedText();
	private static string Number(string key, int value)
	{
		var loc = new LocString("rest_site_ui", "SPOON_SALE." + key);
		loc.Add("Amount", value);
		return loc.GetFormattedText();
	}

	private CampfireSalePanel(NativeTradePopup popup, Player seller, Player buyer, CardModel card)
	{
		_popup = popup;
		_popup.ReservePreviewColumn(300);
		_popup.Closed += () =>
		{
			_terms.TrySetResult(null);
			// Closed from outside without an answer: answer for the player so the sale cannot hang.
			if (_offer != null) Reply(LocalContext.IsMe(_offer.Buyer) ? CampfireSaleKind.Decline : CampfireSaleKind.Cancel);
		};
		NativeTradePopup.Label(_popup.Rows, MerchantLoans.NameOf(seller) + " → " + MerchantLoans.NameOf(buyer));
		// The game's card node keeps its art, upgrades and enchantment visible beside the native popup.
		if (NCard.Create(card) is { } visual)
		{
			_popup.AddChild(visual);
			visual.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
			visual.Position += new Vector2(-500, 0);
			visual.Scale = new Vector2(0.8f, 0.8f);
			visual.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
		}
	}

	public static Task<(int Price, int Rate)?> ChooseTerms(Player seller, Player buyer, CardModel card)
	{
		if (NativeTradePopup.Open(Text("title")) is not { } popup) return Task.FromResult<(int, int)?>(null);
		var panel = new CampfireSalePanel(popup, seller, buyer, card);
		NativeTradePopup.Slider(popup.Rows, Text("price_label"), 0, CampfireGiftRule.MaxPrice, next => panel._price = next);
		NativeTradePopup.Slider(popup.Rows, Text("rate_label"), 0, CampfireGiftRule.MaxRate, next => panel._rate = next);
		NativeTradePopup.Label(popup.Rows, Text("credit"));
		var buttons = NativeTradePopup.Actions(popup.Footer);
		NativeTradePopup.Button(buttons, Text("cancel"), panel.Close, positive: false);
		NativeTradePopup.Button(buttons, Text("offer"), () =>
		{
			panel._terms.TrySetResult((panel._price, panel._rate));
			panel.Close();
		});
		popup.Cancel = panel.Close;
		SpoonWidgets.RestoreFocus(popup, -1, true);
		return panel._terms.Task;
	}

	public static CampfireSalePanel? ShowOffer(CampfireSale sale)
	{
		if (NativeTradePopup.Open(Text("title")) is not { } popup) return null;
		var panel = new CampfireSalePanel(popup, sale.Seller, sale.Buyer, sale.Card) { _offer = sale };
		NativeTradePopup.Label(popup.Rows, Number("price", sale.Price));
		NativeTradePopup.Label(popup.Rows, Number("paid_now", sale.PaidNow));
		NativeTradePopup.Label(popup.Rows, Number("rate", sale.Rate));
		NativeTradePopup.Label(popup.Rows, Number("debt", sale.Debt));
		if (sale.Debt > 0) NativeTradePopup.Label(popup.Rows, Text("repayment"));
		var buttons = NativeTradePopup.Actions(popup.Footer);
		if (LocalContext.IsMe(sale.Buyer))
		{
			NativeTradePopup.Button(buttons, Text("decline"), () => panel.Reply(CampfireSaleKind.Decline), positive: false);
			NativeTradePopup.Button(buttons, Text("accept"), () => panel.Reply(CampfireSaleKind.Accept));
			popup.Cancel = () => panel.Reply(CampfireSaleKind.Decline);
		}
		else
		{
			NativeTradePopup.Label(popup.Rows, Text("waiting"));
			NativeTradePopup.Button(buttons, Text("cancel"), () => panel.Reply(CampfireSaleKind.Cancel), positive: false);
			popup.Cancel = () => panel.Reply(CampfireSaleKind.Cancel);
		}
		SpoonWidgets.RestoreFocus(popup, -1, true);
		return panel;
	}

	private void Reply(CampfireSaleKind kind)
	{
		if (_sent || _offer == null) return;
		_sent = true;
		CampfireSales.Send(LocalContext.IsMe(_offer.Buyer) ? _offer.Buyer : _offer.Seller, new NetCampfireSaleAction {
			Kind = kind, Seller = MerchantLoans.IndexOf(_offer.Seller), SaleId = _offer.Id });
	}

	public void Close() => _popup.Close();

	/// <summary>Closes an offer the sale already settled, without sending an answer.</summary>
	public void Dismiss()
	{
		_sent = true;
		_popup.Close();
	}

	/// <summary>A short message with one button, for a sale that ended without the player's own answer.</summary>
	public static void ShowNotice(string key)
	{
		if (NativeTradePopup.Open(Text("title")) is not { } popup) return;
		NativeTradePopup.Label(popup.Rows, Text(key));
		NativeTradePopup.Button(NativeTradePopup.Actions(popup.Footer), Text("ok"), popup.Close);
		popup.Cancel = popup.Close;
		SpoonWidgets.RestoreFocus(popup, -1, true);
	}
}
