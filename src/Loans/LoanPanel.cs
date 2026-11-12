using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using SlaySpoon.Core;
using SlaySpoon.Ui;

namespace SlaySpoon.Loans;

/// <summary>
/// Local controls only; all transactions go through the shared action queue. Controller: up/down moves between rows,
/// left/right steps a value row like the Custom Run percent rows (hold to step ten times faster), confirm presses a
/// button. Focus comes to the panel when a request appears and stays on the same row across refreshes.
/// </summary>
public sealed class LoanPanel
{
	private NativeTradePopup? _popup;
	private VBoxContainer _rows = null!;
	private readonly Player _me;
	private readonly Dictionary<int, (int Amount, int Rate)> _drafts = new();
	private readonly Dictionary<(int Borrower, int Lender), bool> _picked = new();
	private readonly Dictionary<(int Borrower, int Lender), LoanOffer> _seenOffers = new();
	private bool _queued;
	private bool _closed;
	private bool _wasVisible;
	private bool _dismissed = true;
	private bool _requestEditor;
	private int _selectedBorrower = -1;
	private bool _pending;
	private bool _userDismissed;
	private readonly HashSet<int> _knownRequests = new();
	private string? _status;
	private VBoxContainer _footer = null!;
	private int _requestAmount = 100;
	private readonly Dictionary<int, bool> _gifts = new();
	private readonly NPopupYesNoButton _open;

	public static LocString Loc(string key) => new("merchant_room", "SLAY_SPOON_LOANS." + key);
	private static string Text(string key) => Loc(key).GetFormattedText();
	private static string Number(string key, int value)
	{
		var loc = Loc(key);
		loc.Add("Amount", value);
		return loc.GetFormattedText();
	}

	public LoanPanel(NMerchantRoom room)
	{
		_me = MerchantLoans.Me(MerchantLoans.Run)!;
		_open = NativeTradePopup.Button(room.Inventory, Text("title"), () => { _requestEditor = MerchantLoans.Board.Borrowers.Count == 0; _userDismissed = false; _dismissed = false; Schedule(); });
		_open.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
		_open.Position += new Vector2(-340, 40);
		MerchantLoans.Board.Changed += OnBoardChanged;
		MerchantLoans.Receipt += OnReceipt;
		_me.GoldChanged += Schedule;
		room.TreeExiting += () =>
		{
			_closed = true;
			_popup?.Close();
			MerchantLoans.Board.Changed -= OnBoardChanged;
			MerchantLoans.Receipt -= OnReceipt;
			_me.GoldChanged -= Schedule;
			// Held gold is refunded by LoanBoardRoomExitPatch in synchronized room order.
		};
		Refresh();
	}

	private void OnBoardChanged()
	{
		var borrowers = MerchantLoans.Board.Borrowers;
		bool newRequest = borrowers.Any(b => !_knownRequests.Contains(b));
		_knownRequests.Clear();
		_knownRequests.UnionWith(borrowers);
		if (newRequest && !_userDismissed) _dismissed = false;
		Schedule();
	}

	private void OnReceipt(int player, string key, int amount, int debt)
	{
		if (player != MerchantLoans.IndexOf(_me)) return;
		if (key != "gift_delivered" && key != "loan_delivered") _pending = false;
		var loc = Loc(key);
		loc.Add("Amount", amount);
		loc.Add("Debt", debt);
		_status = loc.GetFormattedText();
		if (key == "request_sent") { _selectedBorrower = player; _requestEditor = false; _dismissed = false; }
		Schedule();
	}

	private int _sendToken;

	private void Send(NetLoanAction action)
	{
		if (_pending || !MerchantLoans.Send(action)) return;
		_pending = true;
		_status = Text("sending");
		// A dropped action never gets a receipt: unlock the buttons again rather than freeze the panel.
		int token = ++_sendToken;
		_open.GetTree()?.CreateTimer(10).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() =>
		{
			if (_closed || !_pending || token != _sendToken) return;
			_pending = false;
			_status = null;
			Schedule();
		}));
		Schedule();
	}

	/// <summary>Escape and Cancel only close the window; withdrawing a request has its own button.</summary>
	private void Cancel() => Dismiss();

	private void WithdrawRequest() => Send(new NetLoanAction { Kind = LoanActionKind.Cancel });

	private void Dismiss()
	{
		_userDismissed = true;
		_dismissed = true;
		_popup?.Close();
	}

	private void Schedule()
	{
		if (_closed || _queued) return;
		_queued = true;
		Callable.From(() => { _queued = false; if (!_closed) Refresh(); }).CallDeferred();
	}

	private static void Label(Control parent, string text) => NativeTradePopup.Label(parent, text);

	private NPopupYesNoButton Button(Control parent, string text, Action action, bool disabled = false, bool positive = true) =>
		NativeTradePopup.Button(parent, text, action, disabled || _pending, positive);

	private void Refresh()
	{
		var board = MerchantLoans.Board;
		_open.Visible = true;
		_open.SetText(Text("title") + (board.Borrowers.Count > 0 ? " (" + board.Borrowers.Count + ")" : ""));
		_open.TooltipText = _status ?? Text("rules");
		foreach (int stale in _drafts.Keys.Where(b => !board.HasRequest(b)).ToArray()) { _drafts.Remove(stale); _gifts.Remove(stale); }
		foreach (var stale in _picked.Keys.Where(k => !board.OffersFor(k.Borrower).Any(o => o.Lender == k.Lender)).ToArray())
			{ _picked.Remove(stale); _seenOffers.Remove(stale); }
		if (_dismissed)
		{
			_popup?.Close();
			_wasVisible = false;
			return;
		}
		bool appeared = !_wasVisible || _popup == null || _popup.IsClosed;
		if (_popup == null || _popup.IsClosed)
		{
			_popup = NativeTradePopup.Open(Text("title"));
			if (_popup == null) return;
			_rows = _popup.Rows;
			_footer = _popup.Footer;
			_popup.Cancel = Cancel;
			var opened = _popup;
			// Closed from outside (overlay stack cleared): stay closed until the player or a new request reopens it.
			opened.Closed += () => { if (opened == _popup) _dismissed = true; };
		}
		_wasVisible = true;
		int focused = Focusables().FindIndex(c => c.HasFocus());
		foreach (Node child in _footer.GetChildren()) { _footer.RemoveChild(child); child.QueueFree(); }
		foreach (Node child in _rows.GetChildren()) { _rows.RemoveChild(child); child.QueueFree(); }
		Label(_rows, Text("rules"));
		if (_status != null) Label(_footer, _status);
		int me = MerchantLoans.IndexOf(_me);
		if (board.HasRequest(me)) _requestEditor = false;
		if (!board.HasRequest(_selectedBorrower))
			_selectedBorrower = board.HasRequest(me) ? me : board.Borrowers.FirstOrDefault(-1);
		if (board.Borrowers.Count > 1 || (_requestEditor && board.Borrowers.Count > 0))
		{
			foreach (int borrower in board.Borrowers)
			{
				int selected = borrower;
				SpoonWidgets.Button(_rows, MerchantLoans.NameOf(MerchantLoans.PlayerAt(_me.RunState, borrower)!),
					() => { _selectedBorrower = selected; _requestEditor = false; Schedule(); },
					_pending || (!_requestEditor && selected == _selectedBorrower));
			}
		}
		if (!board.HasRequest(me) && !_requestEditor && board.Borrowers.Count > 0)
			SpoonWidgets.Button(_rows, Text("request_title"), () => { _requestEditor = true; Schedule(); }, _pending);
		if (!_requestEditor && board.HasRequest(_selectedBorrower)
			&& MerchantLoans.PlayerAt(_me.RunState, _selectedBorrower) is { } player)
		{
			Label(_rows, MerchantLoans.NameOf(player) + " — " + Number("missing", board.Missing(_selectedBorrower)));
			if (_selectedBorrower == me) OwnRequest(me);
			else OtherRequest(_selectedBorrower, me);
		}

		if (!board.HasRequest(me) && (_requestEditor || (board.Borrowers.Count == 0 && _status == null)))
		{
			Label(_rows, Text("request_title"));
			NPopupYesNoButton? ask = null;
			NativeTradePopup.Slider(_rows, Text("amount_label"), _requestAmount,
				Math.Min(65535, Math.Max(1000, _me.RunState.Players.Where(p => p != _me).Sum(p => Math.Max(0, p.Gold)))),
				next =>
				{
					_requestAmount = next;
					if (ask != null) { if (next > 0 && !_pending) ask.Enable(); else ask.Disable(); }
				});
			var actions = NativeTradePopup.Actions(_footer);
			Button(actions, Text("cancel"), Cancel, positive: false);
			ask = Button(actions, Text("ask"), () =>
			{
				if (_requestAmount > 0) Send(new NetLoanAction { Kind = LoanActionKind.Request, Amount = _requestAmount });
			}, _requestAmount <= 0);
		}
		else if (!board.HasRequest(me) && board.Borrowers.Count == 0)
			Button(_footer, Text("cancel"), Cancel, positive: false);

		SpoonWidgets.RestoreFocus(_popup, focused, appeared);
	}

	private List<Control> Focusables() => SpoonWidgets.Focusables(_popup!);

	private void OwnRequest(int borrower)
	{
		var board = MerchantLoans.Board;
		var offers = board.OffersFor(borrower);
		int mask = 0, total = 0;
		foreach (var offer in offers)
		{
			var key = (borrower, offer.Lender);
			// Changed terms require fresh consent, even when the lender stays the same.
			if (!_seenOffers.TryGetValue(key, out var previous) || previous != offer) _picked[key] = false;
			_seenOffers[key] = offer;
			bool picked = _picked.TryGetValue(key, out bool saved) && saved;
			if (picked) { mask |= 1 << offer.Lender; total += offer.Amount; }
			string name = MerchantLoans.NameOf(MerchantLoans.PlayerAt(_me.RunState, offer.Lender)!);
			string details = offer.Gift ? Text("gift") : Number("rate", offer.RatePercent);
			Label(_rows, name + ": " + Number("gold", offer.Amount) + " · " + details);
			NativeTradePopup.Checkbox(_rows, Text("accept"), picked, value => { _picked[key] = value; Schedule(); });
		}
		Label(_rows, Number("selected", Math.Min(total, board.Missing(borrower))));
		// Preview the actual principal taken, including partial final offers.
		int need = board.Missing(borrower), debt = 0;
		foreach (var offer in offers.Where(o => (mask & (1 << o.Lender)) != 0))
		{
			int taken = Math.Min(need, offer.Amount);
			if (!offer.Gift) debt += LoanLedger.Owed(taken, offer.RatePercent);
			need -= taken;
		}
		Label(_rows, Number("debt", debt));
		var buttons = NativeTradePopup.Actions(_footer);
		int revision = board.Revision(borrower);
		Button(buttons, Text("withdraw"), WithdrawRequest, positive: false);
		Button(buttons, Text("close"), Cancel, positive: false);
		Button(buttons, Text("accept"), () => Send(new NetLoanAction {
			Kind = LoanActionKind.Accept, Lenders = mask, Revision = revision }), total < board.Missing(borrower));
	}

	private void OtherRequest(int borrower, int me)
	{
		var board = MerchantLoans.Board;
		var offers = board.OffersFor(borrower);
		var own = offers.FirstOrDefault(o => o.Lender == me);
		int available = (int)Math.Clamp((long)_me.Gold + (own?.Amount ?? 0), 0, 65535);
		if (!_drafts.TryGetValue(borrower, out var draft))
			draft = (Math.Min(available, Math.Max(0, board.Missing(borrower) - offers.Sum(o => o.Amount))), 0);
		draft.Amount = Math.Clamp(draft.Amount, 0, available);
		_drafts[borrower] = draft;
		bool gift = _gifts.TryGetValue(borrower, out bool savedGift) ? savedGift : own?.Gift == true;
		NativeTradePopup.Checkbox(_rows, Text("give_sum"), gift, value => { _gifts[borrower] = value; Schedule(); });
		NPopupYesNoButton? accept = null;
		var debtLabel = NativeTradePopup.Label(_rows, Number("debt", LoanLedger.Owed(draft.Amount, draft.Rate)));
		debtLabel.Visible = !gift;
		NativeTradePopup.Slider(_rows, Text("amount_label"), draft.Amount, available, next =>
		{
			var current = _drafts[borrower];
			_drafts[borrower] = (next, current.Rate);
			if (accept != null) { if (next > 0 && !_pending) accept.Enable(); else accept.Disable(); }
			debtLabel.Text = Number("debt", LoanLedger.Owed(next, current.Rate));
		});
		if (!gift) NativeTradePopup.Slider(_rows, Text("rate_label"), draft.Rate, LoanBoard.MaxRate, next =>
		{
			var current = _drafts[borrower];
			_drafts[borrower] = (current.Amount, next);
			debtLabel.Text = Number("debt", LoanLedger.Owed(current.Amount, next));
		});
		if (own != null) Label(_rows, Number("held", own.Amount));

		var buttons = NativeTradePopup.Actions(_footer);
		void Offer(bool gift, int amount) => Send(new NetLoanAction {
			Kind = LoanActionKind.Offer, Borrower = borrower, Amount = amount, RatePercent = _drafts[borrower].Rate, Gift = gift });
		if (own != null) Button(buttons, Text("withdraw"), () => Offer(false, 0), positive: false);
		Button(buttons, Text("cancel"), Cancel, positive: false);
		accept = Button(buttons, Text("accept"), () => { if (_drafts[borrower].Amount > 0) Offer(gift, _drafts[borrower].Amount); }, draft.Amount <= 0);
	}

}
