namespace SlaySpoon.Core;

public static class WatcherOrobasRule
{
	public static bool UseFallback(bool watcher, bool toothAvailable) => watcher && !toothAvailable;
	public static bool HolyWater(int roll) => roll switch
	{
		0 => true,
		1 => false,
		_ => throw new ArgumentOutOfRangeException(nameof(roll)),
	};
}
