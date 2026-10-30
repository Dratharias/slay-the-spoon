namespace SlaySpoon.Core;

/// <summary>The numbers behind StS1's Watcher stances and Mantra.</summary>
public static class StanceMath
{
	/// <summary>Mantra needed to enter Divinity; the rest carries over.</summary>
	public const int MantraThreshold = 10;

	/// <summary>Multiplier on attack damage the stance holder deals with cards: Wrath 2, Divinity 3.</summary>
	public static decimal DealtMultiplier(Stance stance) => stance switch
	{
		Stance.Wrath => 2m,
		Stance.Divinity => 3m,
		_ => 1m,
	};

	/// <summary>Multiplier on enemy attack damage the stance holder receives: Wrath 2.</summary>
	public static decimal ReceivedMultiplier(Stance stance) => stance == Stance.Wrath ? 2m : 1m;

	/// <summary>Energy gained on leaving <paramref name="from"/>: Calm gives 2, 3 with Violet Lotus.</summary>
	public static int EnergyOnExit(Stance from, bool violetLotus)
	{
		if (from != Stance.Calm)
		{
			return 0;
		}
		return violetLotus ? 3 : 2;
	}

	/// <summary>Energy gained on entering <paramref name="to"/>: Divinity gives 3.</summary>
	public static int EnergyOnEnter(Stance to) => to == Stance.Divinity ? 3 : 0;

	/// <summary>
	/// Mantra after gaining <paramref name="gain"/> on top of <paramref name="current"/>. At the threshold or above,
	/// the holder enters Divinity and keeps what is over the threshold.
	/// </summary>
	public static (int mantra, bool enterDivinity) AddMantra(int current, int gain)
	{
		int total = current + gain;
		return total >= MantraThreshold ? (total - MantraThreshold, true) : (total, false);
	}
}
