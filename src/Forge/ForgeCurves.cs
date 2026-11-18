using SlaySpoon.Core;

namespace SlaySpoon.Forge;

/// <summary>The reviewed per-card rows of <c>data/forge/curves.json</c>, embedded in the DLL and read once.</summary>
public static class ForgeCurves
{
	private static readonly Lazy<IReadOnlyDictionary<string, ForgeCurve>> Table = new(Load);

	public static ForgeCurve? For(string id) => Table.Value.TryGetValue(id, out var row) ? row : null;

	public static IReadOnlyDictionary<string, ForgeCurve> All => Table.Value;

	private static IReadOnlyDictionary<string, ForgeCurve> Load()
	{
		try
		{
			using var stream = typeof(ForgeCurves).Assembly.GetManifestResourceStream("SlaySpoon.forge.curves.json")
				?? throw new InvalidOperationException("embedded curves.json missing");
			using var reader = new StreamReader(stream);
			return ForgeRule.ParseCurves(reader.ReadToEnd());
		}
		catch (Exception e)
		{
			SpoonLog.Error($"forge: curve table unreadable, every card follows the generic rule: {e.Message}");
			return new Dictionary<string, ForgeCurve>();
		}
	}
}
