using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.sts2.Core.Nodes.TopBar;
using SlaySpoon.Act4;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Loans;

namespace SlaySpoon.TopBar;

/// <summary>
/// The mod ships no modifier icons, so the top bar would show the game's "missing" placeholder.
/// Show a label instead: the percent (red when monsters are stronger than vanilla, blue when weaker), or
/// the short name of a card set mode or Neow's Blessings. Merchant Loans shows what the local player owes (red, "-N")
/// or is owed (blue, "+N"), or its short name when there is no debt.
/// The hover tip still names the modifier.
/// </summary>
[HarmonyPatch(typeof(NTopBarModifier), nameof(NTopBarModifier._Ready))]
public static class TopBarPercentPatch
{
	private static readonly AccessTools.FieldRef<NTopBarModifier, MegaCrit.Sts2.Core.Models.ModifierModel> Modifier =
		AccessTools.FieldRefAccess<NTopBarModifier, MegaCrit.Sts2.Core.Models.ModifierModel>("_modifier");
	private static readonly AccessTools.FieldRef<NTopBarModifier, TextureRect> Icon =
		AccessTools.FieldRefAccess<NTopBarModifier, TextureRect>("_icon");

	private const float Padding = 14f;

	private const float KeySize = 36f;

	public static void Postfix(NTopBarModifier __instance)
	{
		switch (Modifier(__instance))
		{
			case SpoonPercentModifier percent:
				ReplaceIcon(__instance, percent.Percent + "%", ScalingMath.Tone(percent.Percent) switch
				{
					> 0 => StsColors.red,
					< 0 => StsColors.blue,
					_ => StsColors.cream,
				});
				break;
			case SpoonCardSet or SpoonSpireSouvenirModifier or SpoonNeowBlessing or SpoonCampfireGift or SpoonStartingChest or SpoonBossRelics or SpoonMatriochka:
				ReplaceIcon(__instance, new LocString("modifiers", Modifier(__instance).Id.Entry + ".short").GetFormattedText(), StsColors.cream);
				break;
			case SpoonMerchantLoans loans:
				ShowDebts(__instance, loans);
				break;
			case SpoonAct4 act4:
				ShowKeys(__instance, act4);
				break;
		}
	}

	private static void ShowDebts(NTopBarModifier __instance, SpoonMerchantLoans loans)
	{
		Label label = ReplaceIcon(__instance, "", StsColors.cream);
		void Refresh()
		{
			if (!GodotObject.IsInstanceValid(label))
			{
				return;
			}
			int me = MerchantLoans.Me(MerchantLoans.Run) is { } player ? MerchantLoans.IndexOf(player) : -1;
			int owes = me < 0 ? 0 : loans.Ledger.DebtOf(me);
			int owed = me < 0 ? 0 : loans.Ledger.OwedTo(me);
			(string text, Color color) = owes > 0 ? ("-" + owes, StsColors.red)
				: owed > 0 ? ("+" + owed, StsColors.blue)
				: (new LocString("modifiers", loans.Id.Entry + ".short").GetFormattedText(), StsColors.cream);
			label.Text = text;
			label.AddThemeColorOverride("font_color", color);
			FitWidth(__instance, label);
		}
		Refresh();
		MerchantLoans.Changed += Refresh;
		__instance.TreeExiting += () => MerchantLoans.Changed -= Refresh;
	}

	/// <summary>Three small key sprites (red, green, blue as in StS1): full opacity when held, dim when missing.</summary>
	private static void ShowKeys(NTopBarModifier __instance, SpoonAct4 act4)
	{
		Icon(__instance).Texture = null;
		Act4Key[] order = { Act4Key.Ruby, Act4Key.Emerald, Act4Key.Sapphire };
		HBoxContainer row = new HBoxContainer { Name = "SpoonKeys", MouseFilter = Control.MouseFilterEnum.Ignore };
		row.AddThemeConstantOverride("separation", 2);
		TextureRect[] rects = new TextureRect[order.Length];
		for (int i = 0; i < order.Length; i++)
		{
			rects[i] = new TextureRect
			{
				// The large sprite cropped to its gem: the small StS1 sprites are mostly transparent padding.
				Texture = new AtlasTexture
				{
					Atlas = GD.Load<Texture2D>(KeySprites.Path(order[i], large: true)),
					Region = new Rect2(70f, 70f, 116f, 116f),
				},
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				CustomMinimumSize = new Vector2(KeySize, KeySize),
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};
			row.AddChild(rects[i]);
		}
		__instance.AddChild(row);
		row.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		__instance.CustomMinimumSize = new Vector2(
			Mathf.Max(order.Length * (KeySize + 2f) + Padding, __instance.CustomMinimumSize.X), __instance.CustomMinimumSize.Y);
		void Refresh()
		{
			if (!GodotObject.IsInstanceValid(row))
			{
				return;
			}
			for (int i = 0; i < order.Length; i++)
			{
				rects[i].Modulate = new Color(1f, 1f, 1f, act4.Keys.Has(order[i]) ? 1f : 0.25f);
			}
		}
		Refresh();
		Act4Keys.Changed += Refresh;
		__instance.TreeExiting += () => Act4Keys.Changed -= Refresh;
	}

	private static Label ReplaceIcon(NTopBarModifier __instance, string text, Color color)
	{
		TextureRect icon = Icon(__instance);
		icon.Texture = null;

		Label label = new Label
		{
			Name = "SpoonPercent",
			Text = text,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		label.AddThemeFontSizeOverride("font_size", 22);
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeColorOverride("font_outline_color", Colors.Black);
		label.AddThemeConstantOverride("outline_size", 8);
		__instance.AddChild(label);
		label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		FitWidth(__instance, label);
		return label;
	}

	/// <summary>The text is wider than the icon slot; widen the entry so neighbours don't overlap it.</summary>
	private static void FitWidth(NTopBarModifier __instance, Label label)
	{
		float width = label.GetMinimumSize().X + Padding;
		__instance.CustomMinimumSize = new Vector2(
			Mathf.Max(width, __instance.CustomMinimumSize.X), __instance.CustomMinimumSize.Y);
	}
}
