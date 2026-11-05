namespace SlaySpoon.Core;

/// <summary>Mantra a player gained this combat (StS1's mantraGained), counted even when it turned into Divinity.</summary>
public sealed class MantraTally
{
	public int Total { get; private set; }

	public void Add(int amount)
	{
		if (amount > 0)
		{
			Total += amount;
		}
	}
}

/// <summary>
/// One <see cref="MantraTally"/> per key, held weakly: a key that is dropped (a finished combat's state) takes its
/// tally with it, and two keys never share one (co-op players).
/// </summary>
public sealed class MantraTallies<TKey> where TKey : class
{
	private readonly System.Runtime.CompilerServices.ConditionalWeakTable<TKey, MantraTally> _tallies = new();

	public int Gained(TKey key) => _tallies.TryGetValue(key, out MantraTally? tally) ? tally.Total : 0;

	public void Record(TKey key, int amount) => _tallies.GetOrCreateValue(key).Add(amount);
}
