using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
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
	private readonly PanelContainer _panel = new() { Name = "SpoonLoans" };
	private readonly VBoxContainer _rows = new();
	private readonly Player _me;
	private readonly Dictionary<int, (int Amount, int Rate)> _drafts = new();
	private readonly Dictionary<(int Borrower, int Lender), bool> _picked = new();
	private bool _queued;
	private bool _closed;
	private bool _wasVisible;

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
		room.AddChild(_panel);
		// Right side, clear of the merchant's card shelves; grows leftwards.
		_panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
		_panel.GrowHorizontal = Control.GrowDirection.Begin;
		_panel.OffsetRight = -20;
		_panel.OffsetTop = 150;
		_panel.CustomMinimumSize = new Vector2(460, 0);
		_panel.AddThemeStyleboxOverride("panel", SpoonWidgets.PanelStyle());
		var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(460, 440),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
		_panel.AddChild(scroll);
		scroll.AddChild(_rows);
		_rows.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_rows.AddThemeConstantOverride("separation", 8);
		MerchantLoans.Board.Changed += Schedule;
		_me.GoldChanged += Schedule;
		room.TreeExiting += () =>
		{
			_closed = true;
			MerchantLoans.Board.Changed -= Schedule;
			_me.GoldChanged -= Schedule;
			// Held gold is refunded by LoanBoardRoomExitPatch in synchronized room order.
		};
		Refresh();
	}

	private void Schedule()
	{
		if (_closed || _queued) return;
		_queued = true;
		Callable.From(() => { _queued = false; if (!_closed) Refresh(); }).CallDeferred();
	}

	private static void Label(Control parent, string text)
	{
		var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		label.AddThemeFontSizeOverride("font_size", 20);
		parent.AddChild(label);
	}

	private static Button Button(Control parent, string text, Action action, bool disabled = false) =>
		SpoonWidgets.Button(parent, text, action, disabled);

	private void Refresh()
	{
		var focusables = Focusables();
		int focused = focusables.FindIndex(c => c.HasFocus());
		foreach (Node child in _rows.GetChildren()) { _rows.RemoveChild(child); child.QueueFree(); }
		var board = MerchantLoans.Board;
		_panel.Visible = board.Borrowers.Count > 0;
		foreach (int stale in _drafts.Keys.Where(b => !board.HasRequest(b)).ToArray()) _drafts.Remove(stale);
		foreach (var stale in _picked.Keys.Where(k => !board.OffersFor(k.Borrower).Any(o => o.Lender == k.Lender)).ToArray())
			_picked.Remove(stale);
		bool appeared = _panel.Visible && !_wasVisible;
		_wasVisible = _panel.Visible;
		if (!_panel.Visible) return;
		Label(_rows, Text("title"));
		Label(_rows, Text("rules"));
		int me = MerchantLoans.IndexOf(_me);
		foreach (int borrower in board.Borrowers.ToArray())
		{
			if (MerchantLoans.PlayerAt(_me.RunState, borrower) is not { } player) continue;
			_rows.AddChild(new HSeparator());
			Label(_rows, MerchantLoans.NameOf(player) + " — " + Number("missing", board.Missing(borrower)));
			if (borrower == me) OwnRequest(borrower);
			else OtherRequest(borrower, me);
		}
		SpoonWidgets.RestoreFocus(_rows, focused, appeared);
	}

	private List<Control> Focusables() => SpoonWidgets.Focusables(_rows);

	private void OwnRequest(int borrower)
	{
		var board = MerchantLoans.Board;
		var offers = board.OffersFor(borrower);
		int mask = 0, total = 0;
		foreach (var offer in offers)
		{
			var key = (borrower, offer.Lender);
			bool picked = !_picked.TryGetValue(key, out bool saved) || saved;
			if (picked) { mask |= 1 << offer.Lender; total += offer.Amount; }
			string name = MerchantLoans.NameOf(MerchantLoans.PlayerAt(_me.RunState, offer.Lender)!);
			string details = offer.Gift ? Text("gift") : Number("rate", offer.RatePercent);
			var toggle = new CheckButton { Text = name + ": " + Number("gold", offer.Amount) + " · " + details,
				ButtonPressed = picked, FocusMode = Control.FocusModeEnum.All };
			toggle.AddThemeFontSizeOverride("font_size", 18);
			_rows.AddChild(toggle);
			toggle.Toggled += value => { _picked[key] = value; Schedule(); };
			toggle.GuiInput += input =>
			{
				if (!input.IsActionPressed(MegaInput.select)) return;
				toggle.AcceptEvent();
				toggle.ButtonPressed = !toggle.ButtonPressed;
			};
		}
		Label(_rows, Number("selected", total));
		// Preview the actual principal taken, including partial final offers.
		int need = board.Missing(borrower), debt = 0;
		foreach (var offer in offers.Where(o => (mask & (1 << o.Lender)) != 0))
		{
			int taken = Math.Min(need, offer.Amount);
			if (!offer.Gift) debt += LoanLedger.Owed(taken, offer.RatePercent);
			need -= taken;
		}
		Label(_rows, Number("debt", debt));
		var buttons = new HBoxContainer();
		_rows.AddChild(buttons);
		Button(buttons, Text("accept"), () => MerchantLoans.Send(new NetLoanAction {
			Kind = LoanActionKind.Accept, Lenders = mask }), total < board.Missing(borrower));
		Button(buttons, Text("cancel"), () => MerchantLoans.Send(new NetLoanAction { Kind = LoanActionKind.Cancel }));
	}

	private void OtherRequest(int borrower, int me)
	{
		var board = MerchantLoans.Board;
		var offers = board.OffersFor(borrower);
		var own = offers.FirstOrDefault(o => o.Lender == me);
		int available = Math.Max(0, _me.Gold + (own?.Amount ?? 0));
		if (!_drafts.TryGetValue(borrower, out var draft))
			draft = (Math.Min(available, Math.Max(0, board.Missing(borrower) - offers.Sum(o => o.Amount))), 0);
		draft.Amount = Math.Clamp(draft.Amount, 0, available);
		_drafts[borrower] = draft;
		Stepper(Number("amount", draft.Amount), delta =>
		{
			_drafts[borrower] = (delta == 0 ? 0 : Math.Clamp(draft.Amount + delta, 0, available), draft.Rate);
			Schedule();
		});
		Stepper(Number("rate", draft.Rate), delta =>
		{
			_drafts[borrower] = (draft.Amount, delta == 0 ? 0 : Math.Clamp(draft.Rate + delta, 0, LoanBoard.MaxRate));
			Schedule();
		}, 1, 10, 100);
		Label(_rows, Number("debt", LoanLedger.Owed(draft.Amount, draft.Rate)));
		if (own != null) Label(_rows, Number("held", own.Amount));
		var buttons = new HBoxContainer();
		_rows.AddChild(buttons);
		void Offer(bool gift, int amount) => MerchantLoans.Send(new NetLoanAction {
			Kind = LoanActionKind.Offer, Borrower = borrower, Amount = amount, RatePercent = draft.Rate, Gift = gift });
		Button(buttons, Text("lend"), () => Offer(false, draft.Amount), draft.Amount <= 0);
		Button(buttons, Text("give"), () => Offer(true, draft.Amount), draft.Amount <= 0);
		Button(buttons, Text("withdraw"), () => Offer(false, 0), own == null);
	}

	private void Stepper(string title, Action<int> change, params int[] steps) =>
		SpoonWidgets.Stepper(_rows, title, Text("reset"), change, steps);
}
