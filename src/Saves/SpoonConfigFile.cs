using Godot;
using SlaySpoon.Core;

namespace SlaySpoon.Saves;

/// <summary>Reads and writes user://mod_configs/SlaySpoon.cfg (next to BaseLib.cfg).</summary>
public static class SpoonConfigFile
{
	private static string FilePath => ProjectSettings.GlobalizePath("user://mod_configs/SlaySpoon.cfg");

	/// <summary>True: copy regular progress into the modded profile; false: keep them separate; null: not asked yet.</summary>
	public static bool? SyncChoice => SpoonConfig.ReadSyncChoice(File.Exists(FilePath) ? File.ReadAllText(FilePath) : null);

	public static void SaveSyncChoice(bool choice)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		File.WriteAllText(FilePath, SpoonConfig.WriteSyncChoice(choice));
	}
}
