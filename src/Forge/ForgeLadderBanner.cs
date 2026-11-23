using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>
/// The Forge ladder on the card inspect screen (spec docs/superpowers/specs/2026-11-23-forge-ladder-banner-design.md):
/// with "View Upgrade" ticked in a Forge run, a panel left of the big card lists every level up to +9; hovering one
/// shows it on the big card. View only.
/// </summary>
[HarmonyPatch(typeof(NInspectCardScreen), "UpdateCardDisplay")]
public static class ForgeLadderBanner
{
	private const float MiniScale = 0.42f;
	private const float CellWidth = 140f;
	private const float CellHeight = 196f;
	private const float Margin = 48f;
	private const string PanelName = "SpoonForgeLadder";

	private static readonly AccessTools.FieldRef<NInspectCardScreen, List<CardModel>> Cards =
		AccessTools.FieldRefAccess<NInspectCardScreen, List<CardModel>>("_cards");
	private static readonly AccessTools.FieldRef<NInspectCardScreen, int> Index =
		AccessTools.FieldRefAccess<NInspectCardScreen, int>("_index");
	private static readonly AccessTools.FieldRef<NInspectCardScreen, NTickbox> Tickbox =
		AccessTools.FieldRefAccess<NInspectCardScreen, NTickbox>("_upgradeTickbox");
	private static readonly AccessTools.FieldRef<NInspectCardScreen, NCard> BigCard =
		AccessTools.FieldRefAccess<NInspectCardScreen, NCard>("_card");
	private static readonly AccessTools.FieldRef<NInspectCardScreen, Control> HoverTipRect =
		AccessTools.FieldRefAccess<NInspectCardScreen, Control>("_hoverTipRect");

	[HarmonyPostfix]
	public static void AfterDisplay(NInspectCardScreen __instance)
	{
		try
		{
			Refresh(__instance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forge ladder: {e}");
			Panel(__instance, create: false)?.QueueFree();
		}
	}

	private static void Refresh(NInspectCardScreen screen)
	{
		var cards = Cards(screen);
		int index = Index(screen);
		if (cards == null || index < 0 || index >= cards.Count) return;
		CardModel card = cards[index];
		int limit = ForgeLevels.Limit(card);
		bool shows = ForgeLadderRule.Shows(Tickbox(screen).IsTicked, ForgeLevels.InForgeRun(card), limit, card.CurrentUpgradeLevel);
		if (!shows)
		{
			Panel(screen, create: false)?.QueueFree();
			return;
		}
		var levels = ForgeLadderRule.Levels(card.CurrentUpgradeLevel, limit);
		Control panel = Panel(screen, create: true)!;
		foreach (Node child in panel.GetChildren()) child.QueueFree();
		Build(screen, panel, card, levels);
	}

	private static Control? Panel(NInspectCardScreen screen, bool create)
	{
		var existing = screen.GetNodeOrNull<Control>(PanelName);
		if (existing != null && !existing.IsQueuedForDeletion()) return existing;
		if (!create) return null;
		if (existing != null) existing.Name = PanelName + "Old";
		var panel = new Control { Name = PanelName, MouseFilter = Control.MouseFilterEnum.Ignore };
		screen.AddChild(panel);
		return panel;
	}

	private static void Build(NInspectCardScreen screen, Control panel, CardModel card, IReadOnlyList<int> levels)
	{
		Vector2 view = screen.GetViewportRect().Size;
		int rows = (levels.Count + ForgeLadderRule.Columns - 1) / ForgeLadderRule.Columns;
		float height = rows * CellHeight + 56f;
		panel.Position = new Vector2(Margin, (view.Y - height) / 2f);
		panel.Size = new Vector2(ForgeLadderRule.Columns * CellWidth, height);

		var title = new Label
		{
			Text = new LocString("card_selection", "SPOON_FORGE_LADDER").GetFormattedText(),
			HorizontalAlignment = HorizontalAlignment.Center,
			Size = new Vector2(ForgeLadderRule.Columns * CellWidth, 40f),
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		title.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.2f));
		title.AddThemeFontSizeOverride("font_size", 22);
		panel.AddChild(title);

		var minis = new List<NCard>();
		for (int i = 0; i < levels.Count; i++)
		{
			int level = levels[i];
			var (column, row) = ForgeLadderRule.Cell(i);
			var center = new Vector2(column * CellWidth + CellWidth / 2f, 56f + row * CellHeight + CellHeight / 2f);
			NCard? mini = NCard.Create(AtLevel(card, level));
			if (mini == null) continue;
			mini.Scale = Vector2.One * MiniScale;
			mini.Position = center;
			mini.MouseFilter = Control.MouseFilterEnum.Ignore;
			panel.AddChild(mini);
			mini.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			minis.Add(mini);

			var hit = new Control
			{
				Position = center - new Vector2(CellWidth, CellHeight) / 2f,
				Size = new Vector2(CellWidth, CellHeight),
				MouseFilter = Control.MouseFilterEnum.Stop,
			};
			NCard shown = mini;
			hit.MouseEntered += () =>
			{
				if (!GodotObject.IsInstanceValid(screen)) return;
				Show(screen, card, level);
				foreach (var m in minis)
					if (GodotObject.IsInstanceValid(m)) m.Modulate = m == shown ? Colors.White : new Color(0.6f, 0.6f, 0.6f);
			};
			panel.AddChild(hit);
		}
		if (levels.Count > 0)
		{
			Show(screen, card, levels[0]);
			for (int i = 1; i < minis.Count; i++) minis[i].Modulate = new Color(0.6f, 0.6f, 0.6f);
		}
	}

	private static CardModel AtLevel(CardModel card, int level)
	{
		var clone = (CardModel)card.MutableClone();
		clone.UpgradePreviewType = CardUpgradePreviewType.Deck;
		ForgeLevels.RaiseTo(clone, level);
		return clone;
	}

	private static void Show(NInspectCardScreen screen, CardModel card, int level)
	{
		CardModel shown = AtLevel(card, level);
		NCard big = BigCard(screen);
		big.Model = shown;
		big.ShowUpgradePreview();
		NHoverTipSet.Clear();
		NHoverTipSet.CreateAndShow(screen, shown.HoverTips)?.SetAlignment(HoverTipRect(screen), HoverTip.GetHoverTipAlignment(screen));
	}
}
