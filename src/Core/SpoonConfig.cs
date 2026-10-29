using System.Text.Json;

namespace SlaySpoon.Core;

/// <summary>
/// The mod's config file content (mod_configs/SlaySpoon.cfg), a flat JSON object of strings like
/// BaseLib's. Holds the progress-sync choice and the Act 4 unlock and preview flags.
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

	public static string WriteSyncChoice(string? json, bool choice) => With(json, SyncKey, choice ? "True" : "False");

	public const string PreviewKey = "Act4Preview";

	/// <summary>The permanent Act 4 unlock is per modded profile.</summary>
	public static string Act4UnlockKey(int profileId) => $"Act4Unlocked.profile{profileId}";

	/// <summary>Every key of the file; empty when the content is null or not a JSON object of strings.</summary>
	public static Dictionary<string, string> Read(string? json)
	{
		if (json != null)
		{
			try
			{
				Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				if (values != null)
				{
					return values;
				}
			}
			catch (JsonException)
			{
			}
		}
		return new Dictionary<string, string>();
	}

	/// <summary>The file content with one key set and every other key kept.</summary>
	public static string With(string? json, string key, string value)
	{
		Dictionary<string, string> values = Read(json);
		values[key] = value;
		return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
	}

	/// <summary>True only for the exact value "True".</summary>
	public static bool ReadFlag(string? json, string key) => Read(json).TryGetValue(key, out string? raw) && raw == "True";
}
