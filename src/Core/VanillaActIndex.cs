namespace SlaySpoon.Core;

/// <summary>
/// Vanilla code that switches on the act index knows acts 0 to 2 only and throws for the Ending (act index 3).
/// Those reads go through <see cref="For"/>: the Ending borrows act 3's entries (multiplayer HP scaling, the rest
/// site's glory_loop animation), as it already borrows Glory's assets.
/// </summary>
public static class VanillaActIndex
{
	/// <summary>Act 3's index, the last act vanilla tables cover.</summary>
	public const int Last = 2;

	public static int For(int actIndex) => actIndex > Last ? Last : actIndex;
}
