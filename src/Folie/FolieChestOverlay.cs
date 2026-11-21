using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Loans;
using SlaySpoon.Ui;

namespace SlaySpoon.Folie;

/// <summary>
/// The local view of a Folie à deux chest: the act's chest opening, then one column per stake (the owner's head, their
/// name, the cards) with a bid button, the bidders' names under the head they locked, then the fights and the result.
/// Display only: the bids travel through <see cref="FolieChest"/>. Never throws; a failure closes the view.
/// </summary>
public sealed class FolieChestOverlay
{
	private const int MaxCardVisuals = 4;
	private const double RoundSeconds = 0.9;
	private const double ResultSeconds = 6;

	private readonly CanvasLayer _layer;
	private readonly IReadOnlyList<Player> _bidders;
	private readonly IReadOnlyList<FolieStake> _stakes;
	private readonly List<Button> _bidButtons = new();
	private readonly List<VBoxContainer> _markers = new();
	private readonly List<Label> _results = new();
	private readonly Label _status;
	private readonly Control _root;
	private readonly List<NCard> _cards = new();
	private readonly Task _opening;
	private TaskCompletionSource<int?>? _pick;

	private static string Text(string key) => new LocString("card_reward_ui", "SPOON_FOLIE." + key).GetFormattedText();

	private static string Named(string key, Player player)
	{
		var loc = new LocString("card_reward_ui", "SPOON_FOLIE." + key);
		loc.Add("Name", MerchantLoans.NameOf(player));
		return loc.GetFormattedText();
	}

	private FolieChestOverlay(CanvasLayer layer, IRunState run, IReadOnlyList<Player> bidders, IReadOnlyList<FolieStake> stakes)
	{
		_layer = layer;
		_bidders = bidders;
		_stakes = stakes;
		var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.75f), MouseFilter = Control.MouseFilterEnum.Stop };
		dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		layer.AddChild(dim);

		var root = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		_root = root;
		root.AddThemeConstantOverride("separation", 16);
		root.Modulate = new Color(1, 1, 1, 0);
		layer.AddChild(root);
		var title = new Label { Text = Text("title"), HorizontalAlignment = HorizontalAlignment.Center };
		title.AddThemeFontSizeOverride("font_size", 34);
		title.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
		root.AddChild(title);

		var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		row.AddThemeConstantOverride("separation", 40);
		root.AddChild(row);
		for (int s = 0; s < stakes.Count; s++)
		{
			row.AddChild(Column(s));
		}
		_status = new Label { Text = Text("choose"), HorizontalAlignment = HorizontalAlignment.Center };
		_status.AddThemeFontSizeOverride("font_size", 22);
		root.AddChild(_status);
		// NCard.UpdateVisuals does nothing before the node is ready: a fresh card from the pool showed the scene's
		// "Broken Card" placeholder. The cards are in the tree now.
		foreach (NCard card in _cards) card.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
		Fit();
		_opening = Open(run, root);
	}

	private Control Column(int stake)
	{
		FolieStake s = _stakes[stake];
		var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Begin, CustomMinimumSize = new Vector2(190, 0) };
		column.AddThemeConstantOverride("separation", 6);
		var head = new TextureRect
		{
			Texture = s.Owner.Character.IconTexture, CustomMinimumSize = new Vector2(96, 96),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
		};
		column.AddChild(head);
		var name = new Label { Text = MerchantLoans.NameOf(s.Owner), HorizontalAlignment = HorizontalAlignment.Center };
		name.AddThemeFontSizeOverride("font_size", 20);
		column.AddChild(name);
		var markers = new VBoxContainer { CustomMinimumSize = new Vector2(0, 54) };
		markers.AddThemeConstantOverride("separation", 0);
		column.AddChild(markers);
		_markers.Add(markers);
		var bid = SpoonWidgets.Button(column, Text("bid"), () => _pick?.TrySetResult(stake), disabled: true);
		bid.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_bidButtons.Add(bid);
		var result = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
		result.AddThemeFontSizeOverride("font_size", 18);
		column.AddChild(result);
		_results.Add(result);

		if (s.Cards.Count <= MaxCardVisuals)
		{
			foreach (CardModel card in s.Cards)
			{
				var holder = new Control { CustomMinimumSize = new Vector2(160, 220), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
				column.AddChild(holder);
				if (NCard.Create(card) is { } visual)
				{
					holder.AddChild(visual);
					visual.Position = holder.CustomMinimumSize / 2;
					visual.Scale = new Vector2(0.5f, 0.5f);
					_cards.Add(visual);
				}
			}
		}
		else
		{
			foreach (CardModel card in s.Cards)
			{
				var line = new Label { Text = card.Title, HorizontalAlignment = HorizontalAlignment.Center };
				line.AddThemeFontSizeOverride("font_size", 16);
				column.AddChild(line);
			}
		}
		return column;
	}

	private async Task Open(IRunState run, Control root)
	{
		await ChestOpeningOverlay.Play(run.Act);
		if (!GodotObject.IsInstanceValid(root)) return;
		Fit();
		Tween fade = root.CreateTween();
		fade.TweenProperty(root, "modulate:a", 1f, 0.25);
	}

	public static FolieChestOverlay? Show(IRunState run, IReadOnlyList<Player> bidders, IReadOnlyList<FolieStake> stakes)
	{
		try
		{
			if (NGame.Instance is not { } game || !GodotObject.IsInstanceValid(game)) return null;
			var layer = new CanvasLayer { Name = "SpoonFolieChest", Layer = 49 };
			game.AddChild(layer);
			return new FolieChestOverlay(layer, run, bidders, stakes);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: chest view failed: {e}");
			return null;
		}
	}

	/// <summary>The local player's bid, once the chest has opened. A view that cannot show buttons bids on stake 0.</summary>
	public async Task<int?> PickLocal()
	{
		try
		{
			await _opening;
			if (!GodotObject.IsInstanceValid(_layer)) return 0;
			_pick = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
			foreach (Button button in _bidButtons) button.Disabled = false;
			_bidButtons.FirstOrDefault()?.GrabFocus();
			int? bid = await _pick.Task;
			foreach (Button button in _bidButtons) button.Disabled = true;
			_status.Text = Text("waiting");
			return bid;
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: local bid failed: {e}");
			return 0;
		}
	}

	public void ShowBid(int bidder, int? stake)
	{
		try
		{
			if (stake is not { } s || !GodotObject.IsInstanceValid(_layer)) return;
			var marker = new Label { Text = MerchantLoans.NameOf(_bidders[bidder]), HorizontalAlignment = HorizontalAlignment.Center };
			marker.AddThemeFontSizeOverride("font_size", 16);
			marker.AddThemeColorOverride("font_color", SpoonWidgets.Gold);
			_markers[s].AddChild(marker);
			Fit();
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: bid marker failed: {e.Message}");
		}
	}

	/// <summary>Plays each fight round (the fighters' hands), writes who took each stake, then closes.</summary>
	public async Task PlayResults(FolieOutcome outcome)
	{
		try
		{
			await _opening;
			_status.Text = "";
			foreach (var (stake, fight) in outcome.Fights.OrderBy(f => f.Key))
			{
				foreach (RelicPickingFightRound round in fight.rounds)
				{
					if (!GodotObject.IsInstanceValid(_layer)) return;
					_results[stake].Text = string.Join("  ", fight.playersInvolved.Select((p, i) =>
						round.moves[i] is { } move ? $"{MerchantLoans.NameOf(p)}: {Text("move_" + move.ToString().ToLowerInvariant())}" : "")
						.Where(t => t.Length > 0));
					await Wait(RoundSeconds);
				}
			}
			for (int s = 0; s < _stakes.Count; s++)
			{
				FolieAward? award = outcome.Awards.Where(a => a.Stake == s).Select(a => (FolieAward?)a).FirstOrDefault();
				_results[s].Text = award is { } a ? Named(a.Consolation ? "consolation" : "won", _bidders[a.Player]) : Text("lost");
			}
			var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			_status.Text = "";
			var button = SpoonWidgets.Button((Control)_status.GetParent(), Text("continue"), () => done.TrySetResult());
			button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
			button.GrabFocus();
			Fit();
			await Task.WhenAny(done.Task, Wait(ResultSeconds));
		}
		catch (Exception e)
		{
			SpoonLog.Error($"folie: chest results failed: {e}");
		}
		finally
		{
			if (GodotObject.IsInstanceValid(_layer)) _layer.QueueFree();
		}
	}

	/// <summary>
	/// Scales the view down until its content fits the screen. Three cards per column plus the heads, bidders and the
	/// Continue button are taller than 1080 px; the button ended below the screen.
	/// </summary>
	private void Fit()
	{
		if (!GodotObject.IsInstanceValid(_root) || !_root.IsInsideTree()) return;
		Vector2 view = _root.GetViewportRect().Size;
		Vector2 need = _root.GetCombinedMinimumSize() + new Vector2(0, FitMargin);
		float scale = Math.Min(1f, Math.Min(view.X / need.X, view.Y / need.Y));
		_root.Position = Vector2.Zero;
		_root.Scale = new Vector2(scale, scale);
		_root.Size = view / scale;
	}

	private const float FitMargin = 40;

	private async Task Wait(double seconds)
	{
		if (!GodotObject.IsInstanceValid(_layer)) return;
		await _layer.ToSignal(_layer.GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}
}
