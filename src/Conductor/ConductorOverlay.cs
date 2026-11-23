using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Rewards;
using SlaySpoon.Core;
using SlaySpoon.Loans;
using SlaySpoon.Ui;

namespace SlaySpoon.Conductor;

/// <summary>
/// The Conductor's screen: every movable reward of the fight on one page (the cards themselves, the relic or potion
/// with its name and text), each with one portrait per living player (a 2 × 2 square from three players); the reward's
/// owner is selected first. The rewards take the column count that shrinks the page least on the current screen
/// (<see cref="ConductorRule.Columns"/>), redone when the window changes size. Confirm returns one recipient slot per
/// reward. Display only: the picks travel through the synced choice. Never throws; a screen that cannot open keeps
/// every reward with its owner.
/// </summary>
public sealed class ConductorOverlay
{
	/// <summary>A portrait: the player's name and character icon.</summary>
	public sealed record Head(string Name, Texture2D? Icon);

	/// <summary>A reward row: its owner's slot, then either cards, or an icon with a name and text.</summary>
	public sealed record Entry(int Owner, IReadOnlyList<CardModel>? Cards, Texture2D? Icon, string Title, string Text);

	public const string LayerName = "SpoonConductor";
	private const float CardScale = 0.55f;
	private const int GapW = 60, GapH = 14, Separation = 18;
	private static readonly Vector2 CardSlot = new(300 * CardScale + 8, 422 * CardScale + 8);
	private static readonly Regex Markup = new(@"\[[^\]]*\]", RegexOptions.Compiled);

	private readonly CanvasLayer _layer;
	private readonly Control _root;
	private readonly Label _title;
	private readonly Label? _hint;
	private readonly GridContainer _grid;
	private readonly Button _confirm;
	private readonly List<Control> _rows = new();
	private readonly int[] _picks;
	private readonly int[] _cardPicks;
	private readonly int _players;
	private readonly List<NCard> _cards = new();
	private readonly TaskCompletionSource<List<int>> _done = new(TaskCreationOptions.RunContinuationsAsynchronously);

	private static string Text(string key) => new LocString("modifiers", "SPOON_CONDUCTOR." + key).GetFormattedText();

	public static Task<List<int>> Ask(IReadOnlyList<(Player owner, Reward reward)> rewards, IReadOnlyList<Player> players)
	{
		var list = players.ToList();
		var owners = rewards.Select(r => Math.Max(0, list.IndexOf(r.owner))).ToList();
		try
		{
			var entries = rewards.Select((r, i) => ToEntry(owners[i], r.reward)).ToList();
			var heads = list.Select(p => new Head(MerchantLoans.NameOf(p), p.Character.IconTexture)).ToList();
			return Show(entries, heads) ?? Task.FromResult(owners);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"conductor: screen failed, rewards stay with their owners: {e}");
			return Task.FromResult(owners);
		}
	}

	/// <summary>Opens the screen under <paramref name="parent"/> (the game node by default); null without one.</summary>
	internal static Task<List<int>>? Show(IReadOnlyList<Entry> entries, IReadOnlyList<Head> heads, Node? parent = null)
	{
		parent ??= NGame.Instance;
		if (parent == null || !GodotObject.IsInstanceValid(parent)) return null;
		var layer = new CanvasLayer { Name = LayerName, Layer = 49 };
		parent.AddChild(layer);
		return new ConductorOverlay(layer, entries, heads)._done.Task;
	}

	private static Entry ToEntry(int owner, Reward reward) => reward switch
	{
		CardReward cards => new Entry(owner, cards.Cards.ToList(), null, "", ""),
		RelicReward { Relic: { } relic } => new Entry(owner, null, relic.BigIcon, relic.Title.GetFormattedText(), relic.HoverTip.Description),
		PotionReward { Potion: { } potion } => new Entry(owner, null, potion.LargeImage, potion.Title.GetFormattedText(), potion.HoverTip.Description),
		_ => new Entry(owner, null, null, reward.Description.GetFormattedText(), ""),
	};

	private ConductorOverlay(CanvasLayer layer, IReadOnlyList<Entry> entries, IReadOnlyList<Head> heads)
	{
		_layer = layer;
		_picks = entries.Select(e => e.Owner).ToArray();
		_cardPicks = entries.Select(_ => -1).ToArray();
		_players = heads.Count;
		var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.8f), MouseFilter = Control.MouseFilterEnum.Stop };
		dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		layer.AddChild(dim);

		var root = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		_root = root;
		root.AddThemeConstantOverride("separation", Separation);
		layer.AddChild(root);
		_title = new Label { Text = Text("ask"), HorizontalAlignment = HorizontalAlignment.Center };
		_title.AddThemeFontSizeOverride("font_size", 34);
		_title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		root.AddChild(_title);
		if (entries.Any(e => e.Cards is { Count: > 0 }))
		{
			var hint = _hint = new Label { Text = Text("hint"), HorizontalAlignment = HorizontalAlignment.Center };
			hint.AddThemeFontSizeOverride("font_size", 20);
			hint.AddThemeColorOverride("font_color", new Color(0.75f, 0.78f, 0.85f));
			root.AddChild(hint);
		}

		_grid = new GridContainer { Columns = 1, SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
		_grid.AddThemeConstantOverride("h_separation", GapW);
		_grid.AddThemeConstantOverride("v_separation", GapH);
		root.AddChild(_grid);
		for (int i = 0; i < entries.Count; i++)
		{
			Control row = Row(i, entries[i], heads);
			_rows.Add(row);
			_grid.AddChild(row);
		}

		_confirm = SpoonWidgets.Button(root, Text("confirm"), () =>
		{
			if (!_done.TrySetResult(_picks.Select((p, i) => ConductorRule.Encode(p, _cardPicks[i], _players)).ToList())) return;
			if (GodotObject.IsInstanceValid(_layer)) _layer.QueueFree();
		});
		_confirm.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_confirm.CustomMinimumSize = new Vector2(220, 56);
		_confirm.AddThemeFontSizeOverride("font_size", 24);

		// NCard.UpdateVisuals does nothing before the node is ready (Folie à deux's "Broken Card"): the cards are in the tree now.
		foreach (NCard card in _cards) card.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
		Layout();
		root.GetViewport().SizeChanged += Layout;
		root.TreeExiting += () =>
		{
			if (GodotObject.IsInstanceValid(root) && root.GetViewport() is { } viewport) viewport.SizeChanged -= Layout;
		};
		_confirm.GrabFocus();
	}

	private Control Row(int index, Entry entry, IReadOnlyList<Head> heads)
	{
		var panel = new PanelContainer();
		var style = new StyleBoxFlat { BgColor = new Color(0.08f, 0.1f, 0.14f, 0.9f), BorderColor = new Color(0.35f, 0.4f, 0.5f) };
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(10);
		style.SetContentMarginAll(12);
		panel.AddThemeStyleboxOverride("panel", style);
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 24);
		panel.AddChild(row);
		row.AddChild(RewardView(index, entry));

		bool compact = heads.Count >= 3;
		var side = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		side.AddThemeConstantOverride("separation", compact ? 4 : 8);
		row.AddChild(side);
		var from = new LocString("modifiers", "SPOON_CONDUCTOR.from");
		from.Add("Name", heads[Math.Clamp(entry.Owner, 0, heads.Count - 1)].Name);
		var fromLabel = new Label { Text = from.GetFormattedText(), HorizontalAlignment = HorizontalAlignment.Center };
		fromLabel.AddThemeFontSizeOverride("font_size", 18);
		fromLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.78f, 0.85f));
		side.AddChild(fromLabel);
		var grid = new GridContainer { Columns = ConductorRule.HeadColumns(heads.Count), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
		grid.AddThemeConstantOverride("h_separation", compact ? 6 : 10);
		grid.AddThemeConstantOverride("v_separation", 6);
		side.AddChild(grid);
		var group = new ButtonGroup();
		var buttons = new List<Button>();
		for (int p = 0; p < heads.Count; p++)
		{
			int slot = p;
			var head = new Button
			{
				Text = heads[p].Name, Icon = heads[p].Icon, ExpandIcon = true,
				IconAlignment = HorizontalAlignment.Center, VerticalIconAlignment = VerticalAlignment.Top,
				ToggleMode = true, ButtonGroup = group, ButtonPressed = p == _picks[index],
				CustomMinimumSize = compact ? new Vector2(96, 104) : new Vector2(112, 132), FocusMode = Control.FocusModeEnum.All,
				ClipText = true, TooltipText = heads[p].Name,
			};
			head.AddThemeFontSizeOverride("font_size", compact ? 14 : 16);
			head.Toggled += on => { if (on) _picks[index] = slot; Shade(buttons); };
			head.GuiInput += input =>
			{
				if (input.IsActionPressed(MegaCrit.Sts2.Core.ControllerInput.MegaInput.select))
				{
					head.AcceptEvent();
					head.ButtonPressed = true;
				}
			};
			grid.AddChild(head);
			buttons.Add(head);
		}
		Shade(buttons);
		return panel;
	}

	/// <summary>The chosen head in full color with a gold name, the others dimmed.</summary>
	private static void Shade(List<Button> buttons)
	{
		foreach (Button b in buttons)
		{
			b.SelfModulate = b.ButtonPressed ? Colors.White : new Color(1, 1, 1, 0.45f);
			b.AddThemeColorOverride("font_color", b.ButtonPressed ? SpoonWidgets.Gold : new Color(0.7f, 0.7f, 0.7f));
			b.AddThemeColorOverride("font_pressed_color", SpoonWidgets.Gold);
		}
	}

	/// <summary>A card reward's cards. Clicking one rings it in gold: that card goes straight into the recipient's deck;
	/// clicking it again lets the recipient choose as usual.</summary>
	private Control RewardView(int index, Entry entry)
	{
		if (entry.Cards is not { } cards) return Item(entry.Icon, entry.Title, entry.Text);
		var line = new HBoxContainer();
		line.AddThemeConstantOverride("separation", 4);
		var rings = new List<Panel>();
		for (int c = 0; c < cards.Count && c < ConductorRule.MaxCards; c++)
		{
			int slot = c;
			var holder = new Control { CustomMinimumSize = CardSlot };
			line.AddChild(holder);
			if (NCard.Create(cards[c]) is { } visual)
			{
				holder.AddChild(visual);
				visual.Position = CardSlot / 2;
				visual.Scale = new Vector2(CardScale, CardScale);
				_cards.Add(visual);
			}
			var ringStyle = new StyleBoxFlat { DrawCenter = false, BorderColor = SpoonWidgets.Gold };
			ringStyle.SetBorderWidthAll(4);
			ringStyle.SetCornerRadiusAll(14);
			var ring = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore, Visible = false };
			ring.AddThemeStyleboxOverride("panel", ringStyle);
			ring.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			holder.AddChild(ring);
			rings.Add(ring);
			// Above the card, so the click never reaches NCard's own hover handling.
			var click = new Control
			{
				MouseFilter = Control.MouseFilterEnum.Stop, FocusMode = Control.FocusModeEnum.All,
				MouseDefaultCursorShape = Control.CursorShape.PointingHand, TooltipText = cards[c].Title,
			};
			click.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			holder.AddChild(click);
			click.GuiInput += input =>
			{
				bool pressed = input is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }
					|| input.IsActionPressed(MegaCrit.Sts2.Core.ControllerInput.MegaInput.select);
				if (!pressed) return;
				click.AcceptEvent();
				_cardPicks[index] = _cardPicks[index] == slot ? -1 : slot;
				for (int r = 0; r < rings.Count; r++) rings[r].Visible = r == _cardPicks[index];
			};
		}
		return line;
	}

	/// <summary>A relic or potion: its big icon, its name and its text (the game's color tags dropped).</summary>
	private static Control Item(Texture2D? icon, string name, string description)
	{
		var line = new HBoxContainer { CustomMinimumSize = new Vector2(CardSlot.X * 3, 0) };
		line.AddThemeConstantOverride("separation", 16);
		if (icon != null)
		{
			line.AddChild(new TextureRect
			{
				Texture = icon, CustomMinimumSize = new Vector2(128, 128), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
			});
		}
		var text = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
		line.AddChild(text);
		var title = new Label { Text = name };
		title.AddThemeFontSizeOverride("font_size", 24);
		title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		text.AddChild(title);
		if (description.Length > 0)
		{
			var body = new Label { Text = Markup.Replace(description, ""), AutowrapMode = TextServer.AutowrapMode.WordSmart,
				CustomMinimumSize = new Vector2(300, 0) };
			body.AddThemeFontSizeOverride("font_size", 18);
			text.AddChild(body);
		}
		return line;
	}

	/// <summary>Picks the column count for this screen, then scales the page down until it fits, centered.</summary>
	private void Layout()
	{
		if (!GodotObject.IsInstanceValid(_root) || !_root.IsInsideTree()) return;
		Vector2 view = _root.GetViewportRect().Size;
		float cellW = 0, cellH = 0;
		foreach (Control row in _rows)
		{
			Vector2 size = row.GetCombinedMinimumSize();
			cellW = Math.Max(cellW, size.X);
			cellH = Math.Max(cellH, size.Y);
		}
		float fixedH = _title.GetCombinedMinimumSize().Y + _confirm.GetCombinedMinimumSize().Y + 2 * Separation
			+ (_hint != null ? _hint.GetCombinedMinimumSize().Y + Separation : 0);
		_grid.Columns = ConductorRule.Columns(_rows.Count, cellW, cellH, fixedH, GapW, GapH, view.X, view.Y);

		Vector2 need = _root.GetCombinedMinimumSize() + new Vector2(40, 40);
		float scale = Math.Min(1f, Math.Min(view.X / need.X, view.Y / need.Y));
		_root.Position = Vector2.Zero;
		_root.Scale = new Vector2(scale, scale);
		_root.Size = view / scale;
	}
}
