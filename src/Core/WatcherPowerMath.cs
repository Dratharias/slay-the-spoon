namespace SlaySpoon.Core;

/// <summary>The small numbers behind several Watcher powers.</summary>
public static class WatcherPowerMath
{
	/// <summary>Deva Form: the gain after the one just paid, <paramref name="amount"/> higher.</summary>
	public static int DevaNextGain(int gain, int amount) => gain + amount;

	/// <summary>Establishment: the cost change for a Retained card, which never takes the cost below 0 (a negative result).</summary>
	public static int EstablishmentReduction(int cost, int amount) => -Math.Min(amount, Math.Max(0, cost));

	/// <summary>Devotion: with no Mantra held and 10 or more to gain, the holder enters Divinity directly.</summary>
	public static bool DevotionEntersDivinity(bool hasMantra, int amount) => !hasMantra && amount >= StanceMath.MantraThreshold;
}
