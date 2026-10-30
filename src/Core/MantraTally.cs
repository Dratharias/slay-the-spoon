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
