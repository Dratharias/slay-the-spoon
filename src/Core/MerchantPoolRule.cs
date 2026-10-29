namespace SlaySpoon.Core;

/// <summary>
/// Vanilla "&lt;Character&gt; Cards" adds the other character's cards to the merchant only when every offered card
/// comes from the player's own pool; StS1 cards live in a separate pool that counts as the character's too.
/// </summary>
public static class MerchantPoolRule
{
	/// <param name="optionPools">The pool of each card the merchant offers.</param>
	/// <param name="characterPool">The player's character pool.</param>
	/// <param name="sts1Pool">That character's StS1 pool, or null when it has none.</param>
	public static bool IsCharacterOffer<TPool>(IEnumerable<TPool> optionPools, TPool characterPool, TPool? sts1Pool)
		where TPool : class
	{
		EqualityComparer<TPool> eq = EqualityComparer<TPool>.Default;
		return optionPools.All(p => eq.Equals(p, characterPool) || (sts1Pool != null && eq.Equals(p, sts1Pool)));
	}
}
