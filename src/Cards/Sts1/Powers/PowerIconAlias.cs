using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// Mod powers get their icons through the patched <c>PowerModel.PackedIconPath</c> getter (<see cref="BorrowedIconPatch"/>).
/// The getter is small and hot: the JIT can inline it, or tiered compilation can drop its detour, and the game then asks
/// for the vanilla path <c>atlases/power_atlas.sprites/&lt;id&gt;.tres</c>, which does not exist, and shows "NOPE"
/// (bot logs, 2026-11-20). As with <see cref="Characters.WatcherTrailAlias"/>, a copy of each mod power's icon takes over
/// its vanilla paths (small and big icon) in Godot's resource cache at startup, so the vanilla getters load the right
/// icon with no patch involved.
/// </summary>
internal static class PowerIconAlias
{
	// Godot's resource cache holds weak references: keep the copies alive.
	private static readonly List<Resource> Aliases = new();

	public static async void Install()
	{
		try
		{
			var tree = (SceneTree)Engine.GetMainLoop();
			while (ModelIdSerializationCache.EntryIdBitSize == 0)
				await tree.ToSignal(tree.CreateTimer(0.25), SceneTreeTimer.SignalName.Timeout);
			var ours = typeof(PowerIconAlias).Assembly;
			int packed = 0, big = 0, failed = 0;
			foreach (var power in ModelDb.AllPowers.Where(p => p.GetType().Assembly == ours))
			{
				string id = power.Id.Entry.ToLowerInvariant();
				try
				{
					if (Alias<Texture2D>(power.PackedIconPath, ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + id + ".tres"), copy: r => (Resource)r.Duplicate())) packed++;
					if (Alias<Texture2D>(power.ResolvedBigIconPath, ImageHelper.GetImagePath("powers/" + id + ".png"), copy: AsImageTexture)) big++;
				}
				catch (Exception e)
				{
					failed++;
					SpoonLog.Error($"power icon alias: {id}: {e.Message}");
				}
			}
			SpoonLog.Info($"power icon alias: {packed} small and {big} big icons aliased, {failed} failed");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"power icon alias failed: {e.Message}");
		}
	}

	private static Resource AsImageTexture(Resource r) =>
		((Texture2D)r).GetImage() is { } image ? ImageTexture.CreateFromImage(image) : (Resource)r.Duplicate();

	/// <summary>Makes <paramref name="vanilla"/> load a copy of <paramref name="source"/>; false when nothing was needed or possible.</summary>
	private static bool Alias<T>(string source, string vanilla, Func<Resource, Resource> copy) where T : Resource
	{
		if (string.IsNullOrEmpty(source) || source == vanilla || source.Contains("missing_power")) return false;
		var loaded = ResourceLoader.Load<T>(source, null, ResourceLoader.CacheMode.Reuse);
		if (loaded == null) return false;
		var alias = copy(loaded);
		alias.TakeOverPath(vanilla);
		Aliases.Add(alias);
		return true;
	}
}
