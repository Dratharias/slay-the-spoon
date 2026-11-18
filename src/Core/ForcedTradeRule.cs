namespace SlaySpoon.Core;

/// <summary>Forced Trade (Custom Run, co-op): who receives each player's card.</summary>
public static class ForcedTradeRule
{
	public static bool Applies(int players) => players >= 2;

	/// <summary>
	/// The receiver slot of <paramref name="giver"/>'s card: <paramref name="roll"/> in [0, players − 2] counts the other
	/// players in slot order. −1 in solo.
	/// </summary>
	public static int Receiver(int giver, int players, int roll)
	{
		if (!Applies(players)) return -1;
		int index = Math.Clamp(roll, 0, players - 2);
		return index >= giver ? index + 1 : index;
	}

	public static string Stream(int floor, int slot) => $"SlaySpoon.ForcedTrade.{floor}.{slot}";
}
