namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// A mod power or potion drawn with a vanilla model's icon: the game reads icons from its own atlas by id, and
/// the mod ships no power art.
/// </summary>
public interface IBorrowedIcon
{
	/// <summary>The vanilla model type (same model family) whose icon is shown.</summary>
	Type IconSource { get; }
}
