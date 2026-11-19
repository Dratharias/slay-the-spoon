using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SlaySpoon.DevConsole;

/// <summary>
/// Workshop art tool, off unless the game is launched with <c>SLAYSPOON_RENDER_DIR</c>: once the main menu is ready,
/// renders what <c>SLAYSPOON_RENDER_CARDS</c> names (comma-separated card class names, <c>Name+</c> for the upgraded card,
/// <c>Name+9</c> for a Forge level, <c>relic:Name</c> for a relic's icon and tooltip) to
/// <c>&lt;dir&gt;/&lt;lang&gt;/&lt;Name&gt;.png</c> (<c>+</c> written <c>Plus</c>, <c>relic:</c> dropped) in English and
/// French, then quits.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class CardRenderer
{
	private const int Scale = 2;
	private static bool _started;

	public static void Postfix(NMainMenu __instance)
	{
		string? dir = System.Environment.GetEnvironmentVariable("SLAYSPOON_RENDER_DIR");
		string? names = System.Environment.GetEnvironmentVariable("SLAYSPOON_RENDER_CARDS");
		if (_started || string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(names)) return;
		_started = true;
		TaskHelper.RunSafely(RenderAll(__instance, dir, names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));
	}

	private static async Task RenderAll(Node host, string dir, string[] names)
	{
		string original = LocManager.Instance.Language;
		foreach (string lang in new[] { "eng", "fra" })
		{
			LocManager.Instance.SetLanguage(lang);
			System.IO.Directory.CreateDirectory(System.IO.Path.Combine(dir, lang));
			foreach (string name in names)
			{
				try
				{
					await Render(host, System.IO.Path.Combine(dir, lang), name);
				}
				catch (Exception e)
				{
					SpoonLog.Error($"render {name} ({lang}) failed: {e}");
				}
			}
		}
		LocManager.Instance.SetLanguage(original);
		SpoonLog.Info("render done");
		host.GetTree().Quit();
	}

	private static Type? Find<T>(string typeName) => AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
		.FirstOrDefault(t => t.Name == typeName && typeof(T).IsAssignableFrom(t) && !t.IsAbstract);

	private static async Task Render(Node host, string dir, string name)
	{
		string file = System.IO.Path.Combine(dir, name.Replace("relic:", "").Replace("+", "Plus") + ".png");
		if (name.StartsWith("relic:"))
		{
			Type? relicType = Find<RelicModel>(name["relic:".Length..]);
			if (relicType == null)
			{
				SpoonLog.Error($"render: no relic class {name}");
				return;
			}
			await Snapshot(host, file, RelicView(ModelDb.GetById<RelicModel>(ModelDb.GetId(relicType))), new Vector2(640, 480));
			return;
		}
		int plus = name.IndexOf('+');
		string typeName = plus < 0 ? name : name[..plus];
		int level = plus < 0 ? 0 : plus == name.Length - 1 ? 1 : int.Parse(name[(plus + 1)..]);
		Type? type = Find<CardModel>(typeName);
		if (type == null)
		{
			SpoonLog.Error($"render: no card class {typeName}");
			return;
		}
		CardModel card = ModelDb.GetById<CardModel>(ModelDb.GetId(type)).ToMutable();
		if (level > 1) Forge.ForgeLevels.RaiseTo(card, level);
		else if (level == 1) card.UpgradeInternal();

		// Margin around the card: its frame and cost orb overflow defaultSize. The tool trims the transparent edge.
		NCard node = NCard.Create(card)!;
		Vector2 size = NCard.defaultSize + new Vector2(80, 80);
		node.Position = size / 2;
		var root = new Control();
		root.AddChild(node);
		await Snapshot(host, file, root, size, () => node.UpdateVisuals(PileType.None, CardPreviewMode.Normal));
	}

	/// <summary>A relic as the game shows it on hover: its big icon left of its tooltip.</summary>
	private static Control RelicView(RelicModel relic)
	{
		var root = new Control();
		root.AddChild(new TextureRect
		{
			Texture = relic.BigIcon,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Position = new Vector2(10, 10),
			Size = new Vector2(240, 240),
		});
		HoverTip tip = relic.HoverTip;
		Control control = PreloadManager.Cache.GetScene("res://scenes/ui/hover_tip.tscn").Instantiate<Control>();
		root.AddChild(control);
		control.Position = new Vector2(265, 20);
		control.Size = new Vector2(360, 0);
		control.GetNode<MegaLabel>("%Title").SetTextAutoSize(tip.Title ?? "");
		control.GetNode<MegaRichTextLabel>("%Description").Text = tip.Description;
		control.GetNode<TextureRect>("%Icon").Texture = tip.Icon;
		control.ResetSize();
		return root;
	}

	private static async Task Snapshot(Node host, string file, Control content, Vector2 size, Action? afterReady = null)
	{
		var viewport = new SubViewport
		{
			Size = new Vector2I((int)size.X * Scale, (int)size.Y * Scale),
			TransparentBg = true,
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
		};
		host.AddChild(viewport);
		content.Scale = Vector2.One * Scale;
		viewport.AddChild(content);
		afterReady?.Invoke();
		for (int i = 0; i < 6; i++)
			await host.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
		viewport.GetTexture().GetImage().SavePng(file);
		viewport.QueueFree();
	}
}
