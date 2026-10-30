namespace SlaySpoon.Core;

/// <summary>Regard reveals the top of the current draw pile, without reshuffling.</summary>
public static class ScryMath
{
	public static int CardsToShow(int amount, int drawPileCount, bool goldenEye)
	{
		if (amount < 0 || drawPileCount <= 0) return 0;
		return (int)Math.Min((long)amount + (goldenEye ? 2 : 0), drawPileCount);
	}
}
