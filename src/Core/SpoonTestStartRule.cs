namespace SlaySpoon.Core;

/// <summary>
/// The main-menu "Test: The Spoon" start (a developer shortcut): who sees the button and what the run starts with.
/// </summary>
public static class SpoonTestStartRule
{
	public const int MaxHp = 350;

	/// <summary>The game's name for a mod read from the <c>mods</c> folder next to the executable.</summary>
	public const string ModsDirectorySource = "ModsDirectory";

	/// <summary>
	/// True only for a mod the game read from <c>&lt;game dir&gt;/mods/</c>: its source is the mods directory and its
	/// folder sits under that directory. A Workshop copy, a missing value or any other folder hides the button.
	/// </summary>
	public static bool IsLocalInstall(string? source, string? modPath, string? executablePath)
	{
		if (source != ModsDirectorySource || string.IsNullOrWhiteSpace(modPath) || string.IsNullOrWhiteSpace(executablePath))
			return false;
		string? gameDir = Path.GetDirectoryName(executablePath.Replace('\\', '/'));
		if (string.IsNullOrEmpty(gameDir)) return false;
		string mods = Normalize(Path.Combine(gameDir, "mods")) + "/";
		string mod = Normalize(modPath) + "/";
		return mod.StartsWith(mods, StringComparison.Ordinal) && mod.Length > mods.Length;
	}

	private static string Normalize(string path) =>
		Path.GetFullPath(path.Replace('\\', '/')).Replace('\\', '/').TrimEnd('/');
}
