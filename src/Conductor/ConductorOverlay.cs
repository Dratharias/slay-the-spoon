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
/// with its name and text), each with one portrait per living player; the reward's owner is selected first. Confirm
/// returns one recipient slot per reward. Display only: the picks travel through the synced choice. Never throws; a
/// screen that cannot open keeps every reward with its owner.
/// </summary>
public sealed class ConductorOverlay
{
	private const float CardScale = 0.55f;
	private static readonly Vector2 CardSlot = new(300 * CardScale + 8, 422 * CardScale + 8);
	private static readonly Regex Markup = new(@"\[[^\]]*\]", RegexOptions.Compiled);

	private readonly CanvasLayer _layer;
	private readonly Control _root;
	private readonly int[] _picks;
	private readonly List<NCard> _cards = new();
	private readonly TaskCompletionSource<List<int>> _done = new(TaskCreationOptions.RunContinuationsAsynchronously);

	private static string Text(string key) => new LocString("modifiers", "SPOON_CONDUCTOR." + key).GetFormattedText();

	public static Task<List<int>> Ask(IReadOnlyList<(Player owner, Reward reward)> rewards, IReadOnlyList<Player> players)
	{
		var owners = rewards.Select(r => Math.Max(0, players.ToList().IndexOf(r.owner))).ToList();
		try
		{
			if (NGame.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return Task.FromResult(owners);
			var layer = new CanvasLayer { Name = "SpoonConductor", Layer = 49 };
			game.AddChild(layer);
			return new ConductorOverlay(layer, rewards, players, owners)._done.Task;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"conductor: screen failed, rewards stay with their owners: {e}");
			return Task.FromResult(owners);
		}
	}

	private ConductorOverlay(CanvasLayer layer, IReadOnlyList<(Player owner, Reward reward)> rewards, IReadOnlyList<Player> players,
		List<int> owners)
	{
		_layer = layer;
		_picks = owners.ToArray();
		var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.8f), MouseFilter = Control.MouseFilterEnum.Stop };
		dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		layer.AddChild(dim);

		var root = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		_root = root;
		root.AddThemeConstantOverride("separation", 18);
		layer.AddChild(root);
		var title = new Label { Text = Text("ask"), HorizontalAlignment = HorizontalAlignment.Center };
		title.AddThemeFontSizeOverride("font_size", 34);
		title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		root.AddChild(title);

		// Two columns of rewards from five on, so a four-player fight (up to twelve rewards) stays readable once fitted.
		var grid = new GridContainer { Columns = rewards.Count > 4 ? 2 : 1, SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
		grid.AddThemeConstantOverride("h_separation", 60);
		grid.AddThemeConstantOverride("v_separation", 14);
		root.AddChild(grid);
		for (int i = 0; i < rewards.Count; i++)
			grid.AddChild(Row(i, rewards[i].owner, rewards[i].reward, players));

		var confirm = SpoonWidgets.Button(root, Text("confirm"), () =>
		{
			if (!_done.TrySetResult(_picks.ToList())) return;
			if (GodotObject.IsInstanceValid(_layer)) _layer.QueueFree();
		});
		confirm.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		confirm.CustomMinimumSize = new Vector2(220, 56);
		confirm.AddThemeFontSizeOverride("font_size", 24);

		// NCard.UpdateVisuals does nothing before the node is ready (Folie à deux's "Broken Card"): the cards are in the tree now.
		foreach (NCard card in _cards) card.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
		Fit();
		confirm.GrabFocus();
	}

	private Control Row(int index, Player owner, Reward reward, IReadOnlyList<Player> players)
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
		row.AddChild(RewardView(reward));

		var side = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		side.AddThemeConstantOverride("separation", 8);
		row.AddChild(side);
		var from = new LocString("modifiers", "SPOON_CONDUCTOR.from");
		from.Add("Name", MerchantLoans.NameOf(owner));
		var fromLabel = new Label { Text = from.GetFormattedText(), HorizontalAlignment = HorizontalAlignment.Center };
		fromLabel.AddThemeFontSizeOverride("font_size", 18);
		fromLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.78f, 0.85f));
		side.AddChild(fromLabel);
		var heads = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		heads.AddThemeConstantOverride("separation", 10);
		side.AddChild(heads);
		var group = new ButtonGroup();
		var buttons = new List<Button>();
		for (int p = 0; p < players.Count; p++)
		{
			int slot = p;
			var head = new Button
			{
				Text = MerchantLoans.NameOf(players[p]), Icon = players[p].Character.IconTexture, ExpandIcon = true,
				IconAlignment = HorizontalAlignment.Center, VerticalIconAlignment = VerticalAlignment.Top,
				ToggleMode = true, ButtonGroup = group, ButtonPressed = p == _picks[index],
				CustomMinimumSize = new Vector2(112, 132), FocusMode = Control.FocusModeEnum.All,
				ClipText = true, TooltipText = MerchantLoans.NameOf(players[p]),
			};
			head.AddThemeFontSizeOverride("font_size", 16);
			head.Toggled += on => { if (on) _picks[index] = slot; Shade(buttons); };
			head.GuiInput += input =>
			{
				if (input.IsActionPressed(MegaCrit.Sts2.Core.ControllerInput.MegaInput.select))
				{
					head.AcceptEvent();
					head.ButtonPressed = true;
				}
			};
			heads.AddChild(head);
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

	private Control RewardView(Reward reward)
	{
		switch (reward)
		{
			case CardReward cards:
			{
				var line = new HBoxContainer();
				line.AddThemeConstantOverride("separation", 4);
				foreach (CardModel card in cards.Cards)
				{
					var holder = new Control { CustomMinimumSize = CardSlot, TooltipText = card.Title };
					line.AddChild(holder);
					if (NCard.Create(card) is not { } visual) continue;
					holder.AddChild(visual);
					visual.Position = CardSlot / 2;
					visual.Scale = new Vector2(CardScale, CardScale);
					_cards.Add(visual);
				}
				return line;
			}
			case RelicReward { Relic: { } relic }:
				return Item(relic.BigIcon, relic.Title.GetFormattedText(), relic.HoverTip.Description);
			case PotionReward { Potion: { } potion }:
				return Item(potion.LargeImage, potion.Title.GetFormattedText(), potion.HoverTip.Description);
			default:
				return new Label { Text = reward.Description.GetFormattedText() };
		}
	}

	/// <summary>A relic or potion: its big icon, its name and its text (the game's color tags dropped).</summary>
	private static Control Item(Texture2D? icon, string name, string description)
	{
		var line = new HBoxContainer { CustomMinimumSize = new Vector2(CardSlot.X * 3, 0) };
		line.AddThemeConstantOverride("separation", 16);
		line.AddChild(new TextureRect
		{
			Texture = icon, CustomMinimumSize = new Vector2(128, 128), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
		});
		var text = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center };
		line.AddChild(text);
		var title = new Label { Text = name };
		title.AddThemeFontSizeOverride("font_size", 24);
		title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		text.AddChild(title);
		var body = new Label { Text = Markup.Replace(description, ""), AutowrapMode = TextServer.AutowrapMode.WordSmart,
			CustomMinimumSize = new Vector2(300, 0) };
		body.AddThemeFontSizeOverride("font_size", 18);
		text.AddChild(body);
		return line;
	}

	/// <summary>Scales the page down until it fits the screen, centered.</summary>
	private void Fit()
	{
		if (!GodotObject.IsInstanceValid(_root) || !_root.IsInsideTree()) return;
		Vector2 view = _root.GetViewportRect().Size;
		Vector2 need = _root.GetCombinedMinimumSize() + new Vector2(40, 40);
		float scale = Math.Min(1f, Math.Min(view.X / need.X, view.Y / need.Y));
		_root.Position = Vector2.Zero;
		_root.Scale = new Vector2(scale, scale);
		_root.Size = view / scale;
	}
}
