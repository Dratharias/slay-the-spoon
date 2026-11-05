using System.Text.Json;
using System.Text.Json.Nodes;

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
		return Read(json).TryGetValue(SyncKey, out string? raw) && bool.TryParse(raw, out bool choice) ? choice : null;
	}

	/// <summary>The new file content, or null when the existing content is unreadable and must not be overwritten.</summary>
	public static string? WriteSyncChoice(string? json, bool choice) => With(json, SyncKey, choice ? "True" : "False");

	public const string TintKey = "CharacterTint";

	/// <summary>The permanent Act 4 unlock is per modded profile.</summary>
	public static string Act4UnlockKey(int profileId) => $"Act4Unlocked.profile{profileId}";

	/// <summary>
	/// Every key of the file as strings (JSON booleans and numbers are accepted; a boolean becomes "True" or "False").
	/// Empty when the content is null or not a JSON object.
	/// </summary>
	public static Dictionary<string, string> Read(string? json) => TryRead(json) ?? new Dictionary<string, string>();

	/// <summary>Null when the content is not a JSON object; null or blank content gives an empty dictionary.</summary>
	private static Dictionary<string, string>? TryRead(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return new Dictionary<string, string>();
		}
		try
		{
			using JsonDocument doc = JsonDocument.Parse(json);
			if (doc.RootElement.ValueKind != JsonValueKind.Object)
			{
				return null;
			}
			Dictionary<string, string> values = new();
			foreach (JsonProperty property in doc.RootElement.EnumerateObject())
			{
				switch (property.Value.ValueKind)
				{
					case JsonValueKind.String:
						values[property.Name] = property.Value.GetString() ?? "";
						break;
					case JsonValueKind.True:
						values[property.Name] = "True";
						break;
					case JsonValueKind.False:
						values[property.Name] = "False";
						break;
					case JsonValueKind.Number:
						values[property.Name] = property.Value.GetRawText();
						break;
				}
			}
			return values;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// The file content with one key set and every other key kept as it is (numbers, arrays and objects included);
	/// null when the existing content is unreadable (do not write).
	/// </summary>
	public static string? With(string? json, string key, string value)
	{
		JsonObject root;
		if (string.IsNullOrWhiteSpace(json))
		{
			root = new JsonObject();
		}
		else
		{
			try
			{
				if (JsonNode.Parse(json) is not JsonObject parsed)
				{
					return null;
				}
				root = parsed;
			}
			catch (Exception e) when (e is JsonException or ArgumentException)
			{
				return null;
			}
		}
		try
		{
			// The node parses lazily: a duplicate key in a hand-edited file only throws here.
			root[key] = value;
			return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
		}
		catch (ArgumentException)
		{
			return null;
		}
	}

	/// <summary>True only for the exact value "True".</summary>
	public static bool ReadFlag(string? json, string key) => Read(json).TryGetValue(key, out string? raw) && raw == "True";
}
