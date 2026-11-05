namespace SlaySpoon.Core;

/// <summary>
/// Mod powers that show their own StS1 icon (cut by <c>art.py --powers</c>) instead of a borrowed vanilla one.
/// Kept free of Godot types so tests can check every path against the repo.
/// </summary>
public static class Sts1PowerIcons
{
	private const string Root = "res://SlaySpoon/images/powers/";

	/// <summary>Lowercase ModelDb ids, the same keys as <c>POWERS</c> in <c>tools/sts1/art.py</c>.</summary>
	public static readonly IReadOnlySet<string> Ids = new HashSet<string> { "sts1_block_return_power" };

	public static bool Has(string modelIdEntry) => Ids.Contains(modelIdEntry.ToLowerInvariant());

	/// <summary>The 64×64 combat icon, used where vanilla reads its power atlas sprite.</summary>
	public static string Packed(string modelIdEntry) => Root + modelIdEntry.ToLowerInvariant() + "_packed.png";

	/// <summary>The 128×128 icon, used for hover tips and the applied/flash effects.</summary>
	public static string Big(string modelIdEntry) => Root + modelIdEntry.ToLowerInvariant() + ".png";
}
