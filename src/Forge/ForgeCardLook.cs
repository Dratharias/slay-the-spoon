using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>
/// How a forged card looks (owner, 2026-11-19). From +2 the title climbs the <see cref="ForgeTitleColors"/> ladder and
/// the frame turns Ancient step by step: the Ancient border, glass, text background and banner fade in over the
/// ordinary ones (<see cref="ForgeTitleColors.AncientBlend"/>) while a gloss sweeps the frame
/// (<c>forge_gloss.gdshader</c>, <see cref="ForgeTitleColors.Gloss"/>). At +9 the card is fully Ancient and its title
/// shimmers like a diamond (<c>forge_diamond.gdshader</c>); its own portrait stays in the ordinary window. Cards below
/// +2 and real Ancient cards are left to the game. View only: the level is on the card, so this works in any mode and
/// in the compendium or run history.
/// </summary>
public static class ForgeCardLook
{
	private const string DiamondPath = "res://SlaySpoon/shaders/forge_diamond.gdshader";
	private const string GlossPath = "res://SlaySpoon/shaders/forge_gloss.gdshader";

	private static readonly AccessTools.FieldRef<NCard, MegaLabel> Title = AccessTools.FieldRefAccess<NCard, MegaLabel>("_titleLabel");
	private static readonly AccessTools.FieldRef<NCard, TextureRect> Frame = AccessTools.FieldRefAccess<NCard, TextureRect>("_frame");
	private static readonly AccessTools.FieldRef<NCard, TextureRect> Banner = AccessTools.FieldRefAccess<NCard, TextureRect>("_banner");
	private static readonly AccessTools.FieldRef<NCard, TextureRect> AncientBorder = AccessTools.FieldRefAccess<NCard, TextureRect>("_ancientBorder");
	private static readonly AccessTools.FieldRef<NCard, TextureRect> AncientGlass = AccessTools.FieldRefAccess<NCard, TextureRect>("_ancientBorderGlassOverlay");
	private static readonly AccessTools.FieldRef<NCard, TextureRect> AncientTextBg = AccessTools.FieldRefAccess<NCard, TextureRect>("_ancientTextBg");
	private static readonly AccessTools.FieldRef<NCard, Control> AncientBanner = AccessTools.FieldRefAccess<NCard, Control>("_ancientBanner");

	private static readonly Color TextBgTint = new("141826");

	private static ShaderMaterial? _diamond;
	private static Shader? _glossShader;
	private static readonly Dictionary<int, ShaderMaterial> GlossByLevel = new();

	/// <summary>The one shimmer material every +9 title shares.</summary>
	public static ShaderMaterial Diamond => _diamond ??= new ShaderMaterial { Shader = GD.Load<Shader>(DiamondPath) };

	private static ShaderMaterial GlossFor(int level)
	{
		level = Math.Min(level, ForgeTitleColors.DiamondLevel);
		if (GlossByLevel.TryGetValue(level, out var material)) return material;
		material = new ShaderMaterial { Shader = _glossShader ??= GD.Load<Shader>(GlossPath) };
		material.SetShaderParameter("strength", ForgeTitleColors.Gloss(level));
		return GlossByLevel[level] = material;
	}

	/// <summary>Freezes every shimmer and gloss at <paramref name="seconds"/> (Workshop GIF frames); negative lets them run.</summary>
	public static void FreezeShine(float seconds)
	{
		Diamond.SetShaderParameter("time_override", seconds);
		for (int level = 2; level <= ForgeTitleColors.DiamondLevel; level++) GlossFor(level).SetShaderParameter("time_override", seconds);
	}

	private static int ForgedLevel(NCard card) =>
		card.Model is { Rarity: not CardRarity.Ancient } model && card.Visibility == ModelVisibility.Visible ? model.CurrentUpgradeLevel : 0;

	private static TitleBanner BannerOf(CardModel model) => model.Rarity switch
	{
		CardRarity.Basic or CardRarity.Common => TitleBanner.Common,
		CardRarity.Uncommon => TitleBanner.Uncommon,
		CardRarity.Rare => TitleBanner.Rare,
		_ => TitleBanner.Other,
	};

	/// <summary>
	/// The Ancient text background by card type. <c>CardModel.AncientTextBg</c> throws for any card that is not Ancient,
	/// and that throw inside the card's redraw crashes the game.
	/// </summary>
	private static Texture2D? AncientTextBgFor(CardType type)
	{
		string name = type switch
		{
			CardType.Attack => "attack",
			CardType.Power => "power",
			CardType.Quest => "quest",
			_ => "skill",
		};
		return ResourceLoader.Load<Texture2D>(ImageHelper.GetImagePath($"atlases/compressed_atlas.sprites/ancient_text_bg_{name}.png.tres"),
			null, ResourceLoader.CacheMode.Reuse);
	}

	[HarmonyPatch(typeof(NCard), "UpdateTitleLabel")]
	public static class TitlePatch
	{
		public static void Postfix(NCard __instance)
		{
			MegaLabel label = Title(__instance);
			int level = ForgedLevel(__instance);
			if (ForgeTitleColors.For(level, BannerOf(__instance.Model!)) is not { } colors)
			{
				if (label.Material == _diamond) label.Material = null;
				return;
			}
			label.AddThemeColorOverride("font_color", new Color(colors.Fill));
			label.AddThemeColorOverride("font_outline_color", new Color(colors.Outline));
			if (colors.Shimmer) label.Material = Diamond;
			else if (label.Material == _diamond) label.Material = null;
		}
	}

	/// <summary>
	/// The frame, after every portrait update (from <c>Reload</c> and <c>UpdateVisuals</c>, so a fusion shows at once).
	/// Vanilla never touches these nodes' <c>Modulate</c> and only sets <c>Visible</c> in <c>Reload</c>, so a card that
	/// is not forged gets both back to the vanilla state.
	/// </summary>
	[HarmonyPatch(typeof(NCard), "UpdatePortrait")]
	public static class FramePatch
	{
		public static void Postfix(NCard __instance)
		{
			if (__instance.Model is not { } model) return;
			int level = ForgedLevel(__instance);
			float blend = ForgeTitleColors.AncientBlend(level);
			TextureRect frame = Frame(__instance), banner = Banner(__instance), border = AncientBorder(__instance),
				glass = AncientGlass(__instance), textBg = AncientTextBg(__instance);
			Control ancientBanner = AncientBanner(__instance);
			if (blend <= 0f)
			{
				if (model.Rarity == CardRarity.Ancient) return;
				// Back to vanilla only where a forged look was applied (a card node is reused for other cards).
				if (border.Material is ShaderMaterial m && GlossByLevel.ContainsValue(m))
				{
					border.Material = null;
					border.Visible = glass.Visible = textBg.Visible = ancientBanner.Visible = false;
					frame.Visible = banner.Visible = true;
				}
				if (FindBacking(frame) is { } backing) backing.Visible = false;
				foreach (CanvasItem item in new CanvasItem[] { frame, banner, border, glass, textBg, ancientBanner }) item.Modulate = Colors.White;
				return;
			}
			var shine = GlossFor(level);
			border.Texture = model.AncientBorder;
			textBg.Texture = AncientTextBgFor(model.Type);
			// The gloss goes on the Ancient border only: the ordinary frame's material carries its character color.
			border.Material = shine;
			border.Visible = glass.Visible = textBg.Visible = true;
			// The Ancient border is a translucent glass made to sit over a full-art portrait: an opaque copy of the ordinary
			// frame stays under it, or the glass shows the dark card back and the top of the card turns black with a hard
			// edge. The real frame is drawn above the Ancient layers (they sit below it in the card scene), so it fades out.
			TextureRect under = Backing(frame, glass);
			under.Texture = model.Frame;
			// In grey: the frame texture carries the character's color, and the Ancient border over it should stay the silver
			// of a real Ancient card.
			under.Material = Greyscale;
			under.Modulate = Colors.White;
			under.Visible = true;
			frame.Visible = true;
			// The banners swap instead of crossfading: their shapes differ, so two translucent banners showed the ordinary
			// banner's straight band as a bar across the Ancient ribbon.
			bool ancient = ForgeTitleColors.AncientBanner(level);
			banner.Visible = !ancient;
			ancientBanner.Visible = ancient;
			banner.Modulate = ancientBanner.Modulate = Colors.White;
			var fadeIn = new Color(1, 1, 1, blend);
			border.Modulate = glass.Modulate = fadeIn;
			// A real Ancient card shows its full-art portrait through this background; a forged card has none behind it,
			// so the background is tinted the dark navy of an Ancient card's body to keep the text readable.
			textBg.Modulate = new Color(TextBgTint, blend);
			frame.Modulate = new Color(1, 1, 1, 1f - blend);
		}
	}

	private static ShaderMaterial? _greyscale;

	private static ShaderMaterial Greyscale => _greyscale ??= new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = """
				shader_type canvas_item;
				void fragment() {
					vec4 c = texture(TEXTURE, UV);
					float g = dot(c.rgb, vec3(0.299, 0.587, 0.114));
					COLOR = vec4(vec3(g) * vec3(0.62, 0.64, 0.70), c.a * COLOR.a);
				}
				""",
		},
	};

	private const string BackingName = "SpoonForgeFrameBacking";

	private static TextureRect? FindBacking(TextureRect frame) => frame.GetParent()?.GetNodeOrNull<TextureRect>(BackingName);

	/// <summary>A copy of the ordinary frame placed just below the Ancient glass, made once per card node.</summary>
	private static TextureRect Backing(TextureRect frame, TextureRect glass)
	{
		if (FindBacking(frame) is { } backing) return backing;
		backing = (TextureRect)frame.Duplicate();
		backing.Name = BackingName;
		Node parent = frame.GetParent();
		parent.AddChild(backing);
		parent.MoveChild(backing, glass.GetIndex());
		return backing;
	}
}
