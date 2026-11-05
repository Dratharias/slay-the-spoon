using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Characters;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher;

/// <summary>
/// StS1's stance feedback for the Watcher: the enter sound, the loop held while in the stance and the colored screen-edge
/// flash (StS1's BorderFlashEffect). StS2 plays its sounds through FMOD banks we cannot extend, so the StS1 Oggs play
/// through Godot players that follow the game's squared Master and SFX sliders. Players are children of her creature
/// node and go away with the combat room; the loop also stops when she dies, leaves the stance or the combat ends.
/// </summary>
[HarmonyPatch]
public static class StanceFx
{
	private const string LoopName = "Sts1StanceLoop";
	private const string Group = "SlaySpoonStanceSfx";
	private static bool _subscribed;

	/// <summary>Plays the feedback for <paramref name="creature"/> entering <paramref name="stance"/>; never throws.</summary>
	public static void Entered(Creature creature, Stance stance)
	{
		try
		{
			Subscribe();
			if (creature.Player?.Character is not Sts1Watcher) return;
			if (NCombatRoom.Instance?.GetCreatureNode(creature) is not { } node || !GodotObject.IsInstanceValid(node)) return;
			StopLoop(node);
			SetAura(node, stance);
			if (stance == Stance.Neutral) return;
			Play(node, WatcherAssets.StanceSound(stance, false), loop: false);
			Play(node, WatcherAssets.StanceSound(stance, true), loop: true);
			Flash(node, stance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"watcher: stance feedback failed: {e}");
		}
	}

	private static void Subscribe()
	{
		if (_subscribed) return;
		_subscribed = true;
		CombatManager.Instance.CombatEnded += _ => StopAllLoops();
	}

	private static void Play(Node parent, string path, bool loop)
	{
		var stream = ResourceLoader.Load<AudioStreamOggVorbis>(path)
			?? throw new InvalidOperationException($"Missing stance sound: {path}");
		// Each Ogg is either a one-shot or a loop, so setting Loop on the cached resource is safe.
		stream.Loop = loop;
		var player = new AudioStreamPlayer { Stream = stream, Bus = "Master" };
		if (loop) player.Name = LoopName;
		else player.Finished += player.QueueFree;
		player.AddToGroup(Group);
		parent.AddChild(player);
		player.VolumeLinear = Volume();
		player.Play();
	}

	private static void StopLoop(Node node)
	{
		if (node.GetNodeOrNull<AudioStreamPlayer>(LoopName) is not { } loop) return;
		// Detach first so a new loop can take the name in the same frame.
		node.RemoveChild(loop);
		loop.QueueFree();
	}

	private static void StopAllLoops()
	{
		try
		{
			if (NGame.Instance?.GetTree() is not { } tree) return;
			foreach (Node node in tree.GetNodesInGroup(Group))
			{
				if (node is AudioStreamPlayer { Name: var name } player && name == LoopName && node.GetParent() is { } parent)
				{
					parent.RemoveChild(player);
					player.QueueFree();
				}
			}
		}
		catch (Exception e)
		{
			SpoonLog.Error($"watcher: stopping stance loops failed: {e}");
		}
	}

	private static float Volume(float? master = null, float? sfx = null)
	{
		var settings = SaveManager.Instance.SettingsSave;
		return StingerVolume.Linear(master ?? settings.VolumeMaster, sfx ?? settings.VolumeSfx);
	}

	private static void UpdateVolume(float? master = null, float? sfx = null)
	{
		try
		{
			if (NGame.Instance?.GetTree() is not { } tree) return;
			float volume = Volume(master, sfx);
			foreach (Node node in tree.GetNodesInGroup(Group))
			{
				if (node is AudioStreamPlayer player) player.VolumeLinear = volume;
			}
		}
		catch (Exception e)
		{
			SpoonLog.Error($"watcher: stance sound volume failed: {e}");
		}
	}

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetMasterVol))]
	[HarmonyPostfix]
	private static void Master(float volume) => UpdateVolume(master: volume);

	[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetSfxVol))]
	[HarmonyPostfix]
	private static void Sfx(float volume) => UpdateVolume(sfx: volume);

	/// <summary>StS1 colors: Calm SKY, Wrath SCARLET, Divinity PINK (libGDX values).</summary>
	private static Color FlashColor(Stance stance) => stance switch
	{
		Stance.Calm => new Color(0.53f, 0.81f, 0.92f),
		Stance.Wrath => new Color(1f, 0.13f, 0f),
		_ => new Color(1f, 0.41f, 0.71f),
	};

	/// <summary>A radial gradient over the screen, clear in the middle and the stance color at the edges, fading out.</summary>
	private static void Flash(Node node, Stance stance)
	{
		Color color = FlashColor(stance);
		var gradient = new Gradient
		{
			Offsets = new[] { 0f, 0.55f, 1f },
			Colors = new[] { color with { A = 0f }, color with { A = 0f }, color },
		};
		var texture = new GradientTexture2D
		{
			Gradient = gradient,
			Fill = GradientTexture2D.FillEnum.Radial,
			FillFrom = new Vector2(0.5f, 0.5f),
			FillTo = new Vector2(1.1f, 0.5f),
			Width = 256,
			Height = 256,
		};
		var layer = new CanvasLayer { Name = "Sts1StanceFlash", Layer = 1 };
		var rect = new TextureRect
		{
			Texture = texture,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.Scale,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Modulate = new Color(1f, 1f, 1f, 0f),
		};
		rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		layer.AddChild(rect);
		node.AddChild(layer);
		Tween tween = rect.CreateTween();
		tween.TweenProperty(rect, "modulate:a", 0.7f, 0.15);
		tween.TweenProperty(rect, "modulate:a", 0f, 0.85);
		tween.TweenCallback(Callable.From(layer.QueueFree));
	}

	private const string AuraName = "Sts1StanceAura";

	/// <summary>
	/// A stand-in for StS1's StanceAuraEffect and stance particles: a soft halo pulsing behind her body and particles
	/// rising around it, in the stance color. Children of the body sprite, whose origin is her ground point.
	/// </summary>
	private static void SetAura(NCreature node, Stance stance)
	{
		if (node.Body is not Node2D body || !GodotObject.IsInstanceValid(body)) return;
		if (body.GetNodeOrNull<Node2D>(AuraName) is { } old)
		{
			body.RemoveChild(old);
			old.QueueFree();
		}
		if (stance == Stance.Neutral) return;
		Color color = FlashColor(stance);
		var centre = new Vector2(WatcherAssets.HitboxX + WatcherAssets.HitboxWidth / 2f, WatcherAssets.HitboxY + WatcherAssets.HitboxHeight / 2f);
		var aura = new Node2D { Name = AuraName, ShowBehindParent = true };

		var halo = new Sprite2D
		{
			Texture = RadialTexture(color, 256, 0.15f),
			Position = centre,
			Scale = new Vector2(WatcherAssets.HitboxWidth * 1.4f / 256f, WatcherAssets.HitboxHeight * 1.3f / 256f),
			Modulate = new Color(1f, 1f, 1f, 0.35f),
		};
		aura.AddChild(halo);

		(int amount, float lifetime, float speed) = stance switch
		{
			Stance.Wrath => (36, 0.9f, 110f),
			Stance.Calm => (18, 2.2f, 35f),
			_ => (28, 1.5f, 60f),
		};
		var particles = new CpuParticles2D
		{
			Amount = amount,
			Lifetime = lifetime,
			Texture = RadialTexture(Colors.White, 16, 0f),
			Position = new Vector2(centre.X, 0f),
			EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
			EmissionRectExtents = new Vector2(WatcherAssets.HitboxWidth / 2f, WatcherAssets.HitboxHeight / 4f),
			Direction = Vector2.Up,
			Spread = 15f,
			Gravity = Vector2.Zero,
			InitialVelocityMin = speed * 0.6f,
			InitialVelocityMax = speed,
			ScaleAmountMin = 0.4f,
			ScaleAmountMax = 1f,
			ColorRamp = new Gradient
			{
				Offsets = new[] { 0f, 0.2f, 1f },
				Colors = new[] { color with { A = 0f }, color, color with { A = 0f } },
			},
		};
		aura.AddChild(particles);
		body.AddChild(aura);

		Tween pulse = halo.CreateTween().SetLoops();
		pulse.TweenProperty(halo, "modulate:a", 0.55f, 1.2).SetTrans(Tween.TransitionType.Sine);
		pulse.TweenProperty(halo, "modulate:a", 0.25f, 1.2).SetTrans(Tween.TransitionType.Sine);
	}

	/// <summary>A radial gradient: <paramref name="color"/> from the centre out to <paramref name="solid"/>, then fading to clear.</summary>
	private static GradientTexture2D RadialTexture(Color color, int size, float solid) => new()
	{
		Gradient = new Gradient
		{
			Offsets = new[] { 0f, solid, 1f },
			Colors = new[] { color, color, color with { A = 0f } },
		},
		Fill = GradientTexture2D.FillEnum.Radial,
		FillFrom = new Vector2(0.5f, 0.5f),
		FillTo = new Vector2(1f, 0.5f),
		Width = size,
		Height = size,
	};

	/// <summary>A creature node rebuilt mid-stance gets its loop back, without the enter sound.</summary>
	internal static void Restore(NCreature node, Stance stance)
	{
		try
		{
			if (stance == Stance.Neutral || node.GetNodeOrNull(LoopName) != null) return;
			Subscribe();
			Play(node, WatcherAssets.StanceSound(stance, true), loop: true);
			SetAura(node, stance);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"watcher: restoring the stance loop failed: {e}");
		}
	}

	/// <summary>The loop stops when she dies.</summary>
	internal static void Died(NCreature node)
	{
		try { StopLoop(node); SetAura(node, Stance.Neutral); }
		catch (Exception e) { SpoonLog.Error($"watcher: stopping the stance loop failed: {e}"); }
	}
}
