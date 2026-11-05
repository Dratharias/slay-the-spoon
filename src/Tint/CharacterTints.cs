using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;
using SlaySpoon.Saves;

namespace SlaySpoon.Tint;

/// <summary>
/// Each player's character color. A player sets theirs in the pause menu (saved in the mod cfg) and shares it through
/// the action queue, lazily: the first time their creature or a map line of theirs appears in a run, and on each
/// change. Nothing is saved in the run; a loaded run shares again. The color goes on the creature like the game's own
/// monster hue variants (a material on the Spine body), with the mod's shader.
/// </summary>
public static class CharacterTints
{
	private static readonly Func<RunManager, RunState?> State =
		AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

	private const string ShaderPath = "res://SlaySpoon/shaders/character_tint.gdshader";

	private static readonly Dictionary<ulong, CharacterTint> Shared = new();
	private static readonly List<Target> Live = [];
	private static IRunState? _sentRun;
	private static Shader? _shader;

	public static IRunState? Run => State(RunManager.Instance);

	/// <summary>The local player's setting, as last saved.</summary>
	public static CharacterTint Local => _local ??= SpoonConfigFile.CharacterTint;

	private static CharacterTint? _local;

	public static CharacterTint For(Player player) =>
		Shared.TryGetValue(player.NetId, out var tint) ? tint
		: Run is { } run && LocalContext.GetMe(run) == player ? Local : CharacterTint.Default;

	/// <summary>Changes the local setting: shown at once on this machine, saved, and shared when <paramref name="share"/>.</summary>
	public static void SetLocal(CharacterTint tint, bool share)
	{
		_local = tint.Clamped();
		if (Run is { } run && LocalContext.GetMe(run) is { } me) Shared[me.NetId] = Local;
		RefreshAll();
		if (!share) return;
		SpoonConfigFile.SaveCharacterTint(Local);
		Send();
	}

	/// <summary>Shares the local setting once per run (called where the run is surely live).</summary>
	public static void EnsureSent()
	{
		if (Run is { } run && run != _sentRun) Send();
	}

	private static void Send()
	{
		if (Run is not { } run || LocalContext.GetMe(run) is not { } me) return;
		_sentRun = run;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new TintAction(me, new NetTint { Code = Local.Encode() }));
	}

	internal static void Apply(Player player, NetTint action)
	{
		Shared[player.NetId] = CharacterTint.Decode(action.Code);
		RefreshAll();
	}

	internal static void ResetRun()
	{
		Shared.Clear();
		_sentRun = null;
	}

	/// <summary>A tinted node: how to find its player and read or set its body material.</summary>
	private sealed class Target(Node owner, Func<Player?> player, Func<Material?> get, Action<Material?> set)
	{
		public readonly Node Owner = owner;
		public readonly Func<Player?> Player = player;
		public readonly Func<Material?> Get = get;
		public readonly Action<Material?> Set = set;
		public bool Saved;
		public Material? Original;
	}

	/// <summary>Tracks a player creature node and colors it now and on every change.</summary>
	internal static void Track(NCreature creature) => Add(new Target(creature, () => creature.Entity?.Player,
		() => creature.Visuals is not { } v ? null : v.SpineBody is { } spine ? spine.GetNormalMaterial() : v.GetCurrentBody().Material,
		material => { if (creature.Visuals is { } v) SetMaterial(v, material); }));

	/// <summary>
	/// Tracks a rest site or merchant character: its Spine bodies (direct SpineSprite children) and the Watcher's static
	/// body take the color.
	/// </summary>
	internal static void TrackBodies(Node2D owner, Player player) => Add(new Target(owner, () => player,
		() => Bodies(owner).Select(b => b.GetClass() == MegaSprite.spineClassName ? new MegaSprite(b).GetNormalMaterial() : b.Material)
			.FirstOrDefault(),
		material =>
		{
			foreach (var body in Bodies(owner))
			{
				if (body.GetClass() == MegaSprite.spineClassName) new MegaSprite(body).SetNormalMaterial(material!);
				else SetStaticMaterial(body, material);
			}
		}));

	private static IEnumerable<Node2D> Bodies(Node owner) => owner.GetChildren().OfType<Node2D>()
		.Where(n => n.GetClass() == MegaSprite.spineClassName || n.Name == Characters.WatcherAssetPatch.BodyNodeName);

	private static void Add(Target target)
	{
		Live.Add(target);
		target.Owner.TreeExiting += () => Live.Remove(target);
		Paint(target);
	}

	private static void RefreshAll()
	{
		foreach (var target in Live.ToArray())
		{
			if (GodotObject.IsInstanceValid(target.Owner)) Paint(target);
		}
		Changed?.Invoke();
	}

	/// <summary>Raised after any player's color changes (map lines already drawn keep their color).</summary>
	public static event Action? Changed;

	private static void Paint(Target target)
	{
		if (target.Player() is not { } player) return;
		var tint = For(player);
		Material? current = target.Get();
		if (!tint.Active)
		{
			if (target.Saved)
			{
				target.Saved = false;
				target.Set(target.Original);
				target.Original = null;
			}
			return;
		}
		if (!target.Saved)
		{
			target.Saved = true;
			target.Original = current;
		}
		var material = current as ShaderMaterial;
		if (material?.Shader != (_shader ??= GD.Load<Shader>(ShaderPath)))
		{
			material = new ShaderMaterial { Shader = _shader };
			target.Set(material);
		}
		var c = player.Character.MapDrawingColor;
		var hue = TintMath.Pure(tint.Hue / 360f);
		material.SetShaderParameter("mode", (int)tint.Mode);
		material.SetShaderParameter("target_color", new Vector3(hue.R, hue.G, hue.B));
		material.SetShaderParameter("target_hue", tint.Hue / 360f);
		material.SetShaderParameter("source_hue", TintMath.HueOf(c.R, c.G, c.B));
		material.SetShaderParameter("threshold", tint.Threshold / 360f);
		material.SetShaderParameter("strength", tint.Strength / 100f);
	}

	private static void SetMaterial(NCreatureVisuals visuals, Material? material)
	{
		if (visuals.SpineBody is { } spine)
		{
			spine.SetNormalMaterial(material!);
			// The Watcher's staff eye is a second skeleton under her body.
			Node2D current = visuals.GetCurrentBody();
			foreach (Node child in current.GetChildren())
			{
				if (child.GetClass() == MegaSprite.spineClassName) new MegaSprite(child).SetNormalMaterial(material!);
			}
			// Her corpse sprite replaces the hidden body on death (WatcherAssetPatch).
			if (visuals.GetNodeOrNull<CanvasItem>(Characters.WatcherAssetPatch.CorpseName) is { } corpse)
			{
				corpse.Material = material;
			}
			return;
		}
		SetStaticMaterial(visuals.GetCurrentBody(), material);
	}

	/// <summary>Static bodies (the Watcher): the body's sprites take the body's material.</summary>
	private static void SetStaticMaterial(CanvasItem body, Material? material)
	{
		body.Material = material;
		foreach (var item in body.FindChildren("*", owned: false).OfType<CanvasItem>())
		{
			if (item.Material == null) item.UseParentMaterial = material != null;
		}
	}

	/// <summary>The map drawing color for this player's lines.</summary>
	public static Color MapColor(Player player, Color color)
	{
		var (r, g, b) = TintMath.MapColor((color.R, color.G, color.B), For(player));
		return new Color(r, g, b, color.A);
	}
}

public struct NetTint : INetAction, IPacketSerializable
{
	public int Code;
	public GameAction ToGameAction(Player player) => new TintAction(player, this);
	public void Serialize(PacketWriter writer) => writer.WriteInt(Code, CharacterTint.Bits);
	public void Deserialize(PacketReader reader) => Code = reader.ReadInt(CharacterTint.Bits);
}

/// <summary>
/// Cosmetic only. It travels through the action synchronizer (mods cannot add network message types), but it never
/// enters the action queue: <see cref="TintApplyNowPatch"/> applies it on arrival. Queued, it would take an action id and
/// a post-action checksum at a moment that differs between peers (a desync when a color changed at a turn start), and
/// when sent while a room loads it would sit in the paused queue that combat start waits on (no turn, no draw).
/// </summary>
public class TintAction(Player player, NetTint action) : GameAction
{
	public override ulong OwnerId => player.NetId;
	public override GameActionType ActionType => GameActionType.Any;
	internal void ApplyNow() => CharacterTints.Apply(player, action);
	protected override Task ExecuteAction()
	{
		ApplyNow();
		return Task.CompletedTask;
	}
	public override INetAction ToNetAction() => action;
	public override string ToString() => $"TintAction for player {player.NetId}: {CharacterTint.Decode(action.Code)}";
}
