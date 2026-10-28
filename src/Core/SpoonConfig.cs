using System.Text.Json;

namespace SlaySpoon.Core;

/// <summary>
/// The mod's config file content (mod_configs/SlaySpoon.cfg), a flat JSON object of strings like
/// BaseLib's. Only holds the progress-sync choice for now.
/// </summary>
public static class SpoonConfig
{
	private const string SyncKey = "SyncRegularProgress";

	/// <summary>The saved choice, or null when the player has not been asked yet (or the file is unreadable).</summary>
	public static bool? ReadSyncChoice(string? json)
	{
		if (json == null)
		{
			return null;
		}
		try
		{
			Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
			if (values != null && values.TryGetValue(SyncKey, out string? raw) && bool.TryParse(raw, out bool choice))
			{
				return choice;
			}
		}
		catch (JsonException)
		{
		}
		return null;
	}

	public static string WriteSyncChoice(bool choice)
	{
		Dictionary<string, string> values = new() { [SyncKey] = choice ? "True" : "False" };
		return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
	}
}
