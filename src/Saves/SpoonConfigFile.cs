using Godot;
using SlaySpoon.Core;

namespace SlaySpoon.Saves;

/// <summary>Reads and writes user://mod_configs/SlaySpoon.cfg (next to BaseLib.cfg).</summary>
public static class SpoonConfigFile
{
	private static string FilePath => ProjectSettings.GlobalizePath("user://mod_configs/SlaySpoon.cfg");

	/// <summary>
	/// The file content: ok with null when the file does not exist yet (first run), not ok when it could not be read
	/// (logged once per failure); in that case nothing may be written.
	/// </summary>
	private static bool TryReadContent(out string? content)
	{
		content = null;
		try
		{
			if (File.Exists(FilePath))
			{
				content = File.ReadAllText(FilePath);
			}
			return true;
		}
		catch (Exception e) when (e is IOException or UnauthorizedAccessException)
		{
			SpoonLog.Error($"Could not read {FilePath}: {e.Message}");
			return false;
		}
	}

	private static string? Content => TryReadContent(out string? content) ? content : null;

	/// <summary>Merge one change into the file. Skips the write if the file cannot be read or is not a JSON object.</summary>
	private static void Update(Func<string?, string?> change)
	{
		if (!TryReadContent(out string? content))
		{
			return;
		}
		string? json = change(content);
		if (json == null)
		{
			SpoonLog.Error($"{FilePath} is not a valid JSON object; leaving it untouched.");
			return;
		}
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
			File.WriteAllText(FilePath, json);
		}
		catch (Exception e) when (e is IOException or UnauthorizedAccessException)
		{
			SpoonLog.Error($"Could not write {FilePath}: {e.Message}");
		}
	}

	/// <summary>True: copy regular progress into the modded profile; false: keep them separate; null: not asked yet.</summary>
	public static bool? SyncChoice => SpoonConfig.ReadSyncChoice(Content);

	public static void SaveSyncChoice(bool choice) => Update(c => SpoonConfig.WriteSyncChoice(c, choice));

	/// <summary>Whether this modded profile has earned the permanent Act 4 unlock. Nothing ever clears it.</summary>
	public static bool Act4Unlocked(int profileId) => SpoonConfig.ReadFlag(Content, SpoonConfig.Act4UnlockKey(profileId));

	public static void SaveAct4Unlocked(int profileId) => Update(c => SpoonConfig.With(c, SpoonConfig.Act4UnlockKey(profileId), "True"));

	/// <summary>Owner preview switch: set Act4Preview to "True" (or true) in the cfg file.</summary>
	public static bool Act4Preview => SpoonConfig.ReadFlag(Content, SpoonConfig.PreviewKey);
}
