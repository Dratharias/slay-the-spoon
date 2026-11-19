using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SlaySpoon.DevConsole;

/// <summary>
/// Workshop art tool, off unless the game is launched with <c>SLAYSPOON_RENDER_DIR</c>: once the main menu is ready,
/// renders the cards named in <c>SLAYSPOON_RENDER_CARDS</c> (class names, comma-separated, a trailing <c>+</c> for the
/// upgraded card) to <c>&lt;dir&gt;/&lt;lang&gt;/&lt;Name&gt;.png</c> in English and French, then quits.
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

	private static async Task Render(Node host, string dir, string name)
	{
		bool upgrade = name.EndsWith('+');
		string typeName = name.TrimEnd('+');
		Type? type = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
			.FirstOrDefault(t => t.Name == typeName && typeof(CardModel).IsAssignableFrom(t) && !t.IsAbstract);
		if (type == null)
		{
			SpoonLog.Error($"render: no card class {typeName}");
			return;
		}
		CardModel card = ModelDb.GetById<CardModel>(ModelDb.GetId(type)).ToMutable();
		if (upgrade) card.UpgradeInternal();

		// Margin around the card: its frame and cost orb overflow defaultSize. The tool trims the transparent edge.
		Vector2 size = (NCard.defaultSize + new Vector2(80, 80)) * Scale;
		var viewport = new SubViewport
		{
			Size = new Vector2I((int)size.X, (int)size.Y),
			TransparentBg = true,
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
		};
		host.AddChild(viewport);
		NCard node = NCard.Create(card)!;
		viewport.AddChild(node);
		node.Position = size / 2;
		node.Scale = Vector2.One * Scale;
		node.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		for (int i = 0; i < 6; i++)
			await host.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
		Image image = viewport.GetTexture().GetImage();
		image.SavePng(System.IO.Path.Combine(dir, name.Replace("+", "Plus") + ".png"));
		viewport.QueueFree();
	}
}
