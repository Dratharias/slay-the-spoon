using System.Text.RegularExpressions;

namespace SlaySpoon.Core;

/// <summary>
/// The ModelDb id entry the game derives from a class name (StringHelper.Slugify), reproduced so tests can
/// check ids and asset names without the game. Saves store cards by this id: never rename a shipped class.
/// </summary>
public static partial class ModelIds
{
	/// <summary>Same pattern as the game's StringHelper.CamelCaseRegex.</summary>
	[GeneratedRegex("([A-Za-z0-9]|\\G(?!^))([A-Z])")]
	private static partial Regex CamelCase();

	[GeneratedRegex("\\s+")]
	private static partial Regex Whitespace();

	[GeneratedRegex("[^A-Z0-9_]")]
	private static partial Regex SpecialChars();

	public static string FromClassName(string className)
	{
		string text = CamelCase().Replace(className.Trim(), "$1_$2");
		return SpecialChars().Replace(Whitespace().Replace(text.ToUpperInvariant(), "_"), "");
	}
}
