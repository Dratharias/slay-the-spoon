namespace SlaySpoon.Core;

/// <summary>
/// Save-store paths of progress.save, mirroring UserDataPathProvider.GetProfileDir and
/// ProgressSaveManager.GetProgressPathForProfile (StS2 v0.107.1).
/// </summary>
public static class ProgressPaths
{
	public static string Vanilla(int profileId)
	{
		return $"profile{profileId}/saves/progress.save";
	}

	public static string Modded(int profileId)
	{
		return $"modded/profile{profileId}/saves/progress.save";
	}
}
