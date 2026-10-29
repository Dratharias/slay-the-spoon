using Godot;
using SlaySpoon.Core;

namespace SlaySpoon.Saves;

/// <summary>Reads and writes user://mod_configs/SlaySpoon.cfg (next to BaseLib.cfg).</summary>
public static class SpoonConfigFile
{
	private static string FilePath => ProjectSettings.GlobalizePath("user://mod_configs/SlaySpoon.cfg");

	/// <summary>True: copy regular progress into the modded profile; false: keep them separate; null: not asked yet.</summary>
	public static bool? SyncChoice => SpoonConfig.ReadSyncChoice(File.Exists(FilePath) ? File.ReadAllText(FilePath) : null);

	private static string? Content => File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;

	private static void Write(string json)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		File.WriteAllText(FilePath, json);
	}

	public static void SaveSyncChoice(bool choice) => Write(SpoonConfig.WriteSyncChoice(Content, choice));

	/// <summary>Whether this modded profile has earned the permanent Act 4 unlock. Nothing ever clears it.</summary>
	public static bool Act4Unlocked(int profileId) => SpoonConfig.ReadFlag(Content, SpoonConfig.Act4UnlockKey(profileId));

	public static void SaveAct4Unlocked(int profileId) => Write(SpoonConfig.With(Content, SpoonConfig.Act4UnlockKey(profileId), "True"));

	/// <summary>Owner preview switch: set Act4Preview to "True" in the cfg file.</summary>
	public static bool Act4Preview => SpoonConfig.ReadFlag(Content, SpoonConfig.PreviewKey);
}
