using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace SlaySpoon.Characters;

/// <summary>
/// The Watcher borrows Necrobinder's card trail through the patched <c>CharacterModel.TrailPath</c> getter
/// (<see cref="WatcherAssetPatch"/>). The getter is hot (every card that flies between piles reads it), and the runtime
/// can recompile it without its Harmony detour: it then returns the vanilla path <c>vfx/card_trail_sts1_watcher</c>, a
/// scene that does not exist, and the game crashed right after failing to load it (co-op bot run, 1.8.22). Here a copy
/// of Necrobinder's trail scene takes over that path in Godot's resource cache, so the vanilla path loads too, with no
/// patch involved.
/// </summary>
internal static class WatcherTrailAlias
{
	// Godot's resource cache holds weak references: keep the copy alive.
	private static Resource? _alias;

	public static async void Install()
	{
		try
		{
			var tree = (SceneTree)Engine.GetMainLoop();
			// The model DB is built after mod init (see SerializationPatchGuard).
			while (ModelIdSerializationCache.EntryIdBitSize == 0)
				await tree.ToSignal(tree.CreateTimer(0.25), SceneTreeTimer.SignalName.Timeout);
			string path = SceneHelper.GetScenePath("vfx/card_trail_" + ModelDb.Character<Sts1Watcher>().Id.Entry.ToLowerInvariant());
			string donor = SceneHelper.GetScenePath("vfx/card_trail_" + ModelDb.Character<Necrobinder>().Id.Entry.ToLowerInvariant());
			if (ResourceLoader.Exists(path))
				return;
			var scene = ResourceLoader.Load<PackedScene>(donor, null, ResourceLoader.CacheMode.Reuse);
			if (scene == null)
			{
				SpoonLog.Error($"Watcher trail alias: {donor} did not load");
				return;
			}
			_alias = scene.Duplicate();
			_alias.TakeOverPath(path);
			bool loads = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse) == _alias;
			SpoonLog.Info($"Watcher trail alias: {path} -> {donor}, load check {(loads ? "ok" : "FAILED")}");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"Watcher trail alias failed: {e.Message}");
		}
	}
}
