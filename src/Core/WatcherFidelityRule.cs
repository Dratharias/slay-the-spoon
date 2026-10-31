namespace SlaySpoon.Core;

/// <summary>StS1 Watcher rules independent of engine hooks.</summary>
public static class WatcherFidelityRule
{
	public static bool MasterReality(bool createdByHolder, bool upgradable, bool statusOrCurse, bool foreignInfluence) =>
		createdByHolder && upgradable && !statusOrCurse && !foreignInfluence;

	// Each component was independently modified and floored by the engine, as in StS1 Halt.applyPowers.
	public static decimal HaltBlock(decimal normal, decimal wrathExtra) => Math.Max(0, normal) + Math.Max(0, wrathExtra);
}
