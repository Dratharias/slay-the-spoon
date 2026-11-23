using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
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
/// French, then quits. <c>gif:Name</c> writes the frames of the card's Forge climb instead (<see cref="RenderClimb"/>).
/// <c>elites</c> writes every elite monster in its bestiary pose to <c>&lt;dir&gt;/elites/&lt;ID&gt;.png</c> (once, not per
/// language), for the Trophy portraits (<c>tools/trophy_art.py</c>). <c>conductor</c> shows the Conductor screen with
/// sample rewards for 2, 3 and 4 players on 16:9, 16:10 and 21:9 screens, saved to
/// <c>&lt;dir&gt;/&lt;lang&gt;/conductor-&lt;players&gt;p-&lt;shape&gt;.png</c>.
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
		try
		{
			foreach (string lang in new[] { "eng", "fra" })
			{
				await SetLanguage(host, lang);
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
			await SetLanguage(host, original);
			SpoonLog.Info("render done");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"render stopped: {e}");
		}
		finally
		{
			host.GetTree().Quit();
		}
	}

	/// <summary>
	/// <c>SetLanguage</c> now and then fails to find one of the game's own tables right after a batch of renders
	/// (<c>LocException</c> on <c>eng/card_modifiers.json</c>); a second try a moment later loads it.
	/// </summary>
	private static async Task SetLanguage(Node host, string lang)
	{
		for (int attempt = 1; ; attempt++)
		{
			try
			{
				LocManager.Instance.SetLanguage(lang);
				return;
			}
			catch (Exception e) when (attempt < 5)
			{
				SpoonLog.Info($"render: SetLanguage({lang}) attempt {attempt} failed: {e.Message}");
				await host.ToSignal(host.GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
			}
		}
	}

	private static Type? Find<T>(string typeName) => AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
		.FirstOrDefault(t => t.Name == typeName && typeof(T).IsAssignableFrom(t) && !t.IsAbstract);

	private static async Task Render(Node host, string dir, string name)
	{
		if (name == "elites")
		{
			if (System.IO.Path.GetFileName(dir) == "eng") await RenderElites(host, System.IO.Path.Combine(dir, "..", "elites"));
			return;
		}
		if (name == "conductor")
		{
			await RenderConductor(host, dir);
			return;
		}
		if (name.StartsWith("gif:"))
		{
			await RenderClimb(host, dir, name["gif:".Length..]);
			return;
		}
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
		CardModel card = AtLevel(type, level);

		// Margin around the card: its frame and cost orb overflow defaultSize. The tool trims the transparent edge.
		NCard node = NCard.Create(card)!;
		Vector2 size = NCard.defaultSize + new Vector2(80, 80);
		node.Position = size / 2;
		var root = new Control();
		root.AddChild(node);
		await Snapshot(host, file, root, size, () => node.UpdateVisuals(PileType.None, CardPreviewMode.Normal));
	}

	private static IEnumerable<MonsterModel> EliteMonsters()
	{
		var modEncounters = typeof(CardRenderer).Assembly.GetTypes()
			.Where(t => typeof(EncounterModel).IsAssignableFrom(t) && !t.IsAbstract)
			.Select(t => { try { return ModelDb.GetById<EncounterModel>(ModelDb.GetId(t)); } catch { return null; } })
			.OfType<EncounterModel>();
		return ModelDb.AllEncounters.Concat(modEncounters)
			.Where(e => e.RoomType == RoomType.Elite)
			.SelectMany(e => e.AllPossibleMonsters)
			.DistinctBy(m => m.Id.Entry);
	}

	private static async Task RenderElites(Node host, string dir)
	{
		System.IO.Directory.CreateDirectory(dir);
		foreach (MonsterModel canonical in EliteMonsters())
		{
			try
			{
				// As the bestiary does: a mutable copy on chaotic RNG, set up for combat, in a null combat state.
				MonsterModel monster = canonical.ToMutable();
				monster.Rng = Rng.Chaotic;
				monster.RunRng = new RunRngSet(Rng.Chaotic);
				monster.SetUpForCombat();
				var creature = new Creature(monster, CombatSide.Enemy, null) { CombatState = NullCombatState.Instance };
				NCreature node = NCreature.Create(creature)!;
				var size = new Vector2(900, 900);
				var root = new Control();
				root.AddChild(node);
				await Snapshot(host, System.IO.Path.Combine(dir, canonical.Id.Entry + ".png"), root, size, () =>
				{
					node.SetupForBestiary();
					node.Position = new Vector2(size.X / 2, size.Y - 60);
				});
				SpoonLog.Info($"render: elite {canonical.Id.Entry}");
			}
			catch (Exception e)
			{
				SpoonLog.Error($"render: elite {canonical.Id.Entry} failed: {e}");
			}
		}
	}

	/// <summary>Screen shapes the Conductor preview tries, as the game sees them (its height is always 1080): 16:9,
	/// 16:10 (Steam Deck), 21:9 ultrawide.</summary>
	private static readonly (string name, Vector2I size)[] PreviewSizes =
		{ ("16x9", new(1920, 1080)), ("16x10", new(1728, 1080)), ("21x9", new(2580, 1080)) };

	private static async Task RenderConductor(Node host, string dir)
	{
		var characters = ModelDb.AllCharacters.Take(4).ToList();
		var cards = characters.SelectMany(c => c.CardPool.AllCards.Take(12)).ToList();
		var relics = ModelDb.AllRelics.Take(4).ToList();
		var potions = ModelDb.AllPotions.Take(4).ToList();
		foreach (int players in new[] { 2, 3, 4 })
		{
			var heads = characters.Take(players)
				.Select((c, i) => new Conductor.ConductorOverlay.Head(i == 0 ? "Test Host" : $"Player {i + 1}", c.IconTexture)).ToList();
			// Per player: a card reward, a potion and a relic (an elite fight). Four players: 12 rewards, the worst case.
			var entries = new List<Conductor.ConductorOverlay.Entry>();
			for (int p = 0; p < players; p++)
			{
				entries.Add(new(p, cards.Skip(p * 12 + 3).Take(3).Select(c => c.ToMutable()).ToList(), null, "", ""));
				PotionModel potion = potions[p];
				entries.Add(new(p, null, potion.LargeImage, potion.Title.GetFormattedText(), potion.HoverTip.Description));
				RelicModel relic = relics[p];
				entries.Add(new(p, null, relic.BigIcon, relic.Title.GetFormattedText(), relic.HoverTip.Description));
			}
			foreach (var (name, size) in PreviewSizes)
			{
				var viewport = new SubViewport { Size = size, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
				host.AddChild(viewport);
				var backdrop = new ColorRect { Color = new Color(0.2f, 0.25f, 0.3f), Size = size };
				viewport.AddChild(backdrop);
				_ = Conductor.ConductorOverlay.Show(entries, heads, viewport);
				for (int i = 0; i < 8; i++)
					await host.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
				string file = System.IO.Path.Combine(dir, $"conductor-{players}p-{name}.png");
				viewport.GetTexture().GetImage().SavePng(file);
				SpoonLog.Info($"render: {file}");
				viewport.QueueFree();
				await host.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			}
		}
	}

	private static CardModel AtLevel(Type type, int level)
	{
		CardModel card = ModelDb.GetById<CardModel>(ModelDb.GetId(type)).ToMutable();
		if (level > 1) Forge.ForgeLevels.RaiseTo(card, level);
		else if (level == 1) card.UpgradeInternal();
		return card;
	}

	private const int ClimbFps = 12;
	private const float ClimbHold = 1.5f;

	/// <summary>
	/// The card from +0 to +9, each level held <see cref="ClimbHold"/> seconds and brought in by the game's upgrade
	/// animation (vfx_card_upgrade: the card scales in from nothing over 0.25 s while its particles burst; the vanilla
	/// node itself reads the card's pile, which a card outside a run lacks). Frames are captured in real time at
	/// <see cref="ClimbFps"/> to <c>&lt;dir&gt;/gif_&lt;Name&gt;/NNN.png</c>, at scale 1.
	/// </summary>
	private static async Task RenderClimb(Node host, string dir, string typeName)
	{
		Type? type = Find<CardModel>(typeName);
		if (type == null)
		{
			SpoonLog.Error($"render: no card class {typeName}");
			return;
		}
		string outDir = System.IO.Path.Combine(dir, "gif_" + typeName);
		System.IO.Directory.CreateDirectory(outDir);
		Vector2 size = NCard.defaultSize + new Vector2(200, 200);
		var viewport = new SubViewport
		{
			Size = new Vector2I((int)size.X, (int)size.Y),
			TransparentBg = true,
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
		};
		host.AddChild(viewport);
		var root = new Control();
		viewport.AddChild(root);
		var frames = new List<Image>();
		NCard? shown = null;
		for (int level = 0; level <= Core.ForgeRule.MaxLevel; level++)
		{
			shown?.QueueFree();
			NCard node = NCard.Create(AtLevel(type, level))!;
			node.Position = size / 2;
			root.AddChild(node);
			node.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			shown = node;
			if (level > 0)
			{
				node.Scale = Vector2.Zero;
				node.CreateTween().TweenProperty(node, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				if (UpgradeParticles() is { } particles)
				{
					particles.Position = size / 2;
					root.AddChild(particles);
					particles.Emitting = true;
					particles.Finished += particles.QueueFree;
				}
			}
			for (int i = 0; i < (int)(ClimbHold * ClimbFps); i++)
			{
				await host.ToSignal(host.GetTree().CreateTimer(1.0 / ClimbFps), SceneTreeTimer.SignalName.Timeout);
				frames.Add(viewport.GetTexture().GetImage());
			}
		}
		for (int i = 0; i < frames.Count; i++) frames[i].SavePng(System.IO.Path.Combine(outDir, $"{i:000}.png"));
		viewport.QueueFree();
	}

	/// <summary>The particles of the game's upgrade vfx, taken out of its scene before the scene enters the tree.</summary>
	private static CpuParticles2D? UpgradeParticles()
	{
		Node vfx = GD.Load<PackedScene>(SceneHelper.GetScenePath("vfx/vfx_card_upgrade")).Instantiate();
		var particles = vfx.FindChild("Particle", true, false) as CpuParticles2D;
		particles?.GetParent().RemoveChild(particles);
		if (particles != null) particles.Owner = null;
		vfx.Free();
		return particles;
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
