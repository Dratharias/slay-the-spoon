namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// A mod power drawn with a vanilla power's icon: the game reads power icons from its own atlas by id, and
/// the mod ships no power art.
/// </summary>
public interface IBorrowedIcon
{
	/// <summary>The vanilla PowerModel type whose icon is shown.</summary>
	Type IconSource { get; }
}
