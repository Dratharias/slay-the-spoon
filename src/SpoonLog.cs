using MegaCrit.Sts2.Core.Logging;

namespace SlaySpoon;

/// <summary>Writes to the game's log with a recognizable prefix.</summary>
public static class SpoonLog
{
	public static void Info(string message)
	{
		Log.Info("[SlaySpoon] " + message);
	}
}
