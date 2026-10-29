namespace SlaySpoon.Core;

/// <summary>When the permanent Act 4 unlock is earned.</summary>
public static class Act4UnlockRule
{
	/// <summary>Reaching Act 3 (act index 2) or later in a Standard run at Ascension 10 or higher; victory is not needed.</summary>
	public static bool ShouldUnlock(bool standard, int ascension, int actIndex) => standard && ascension >= 10 && actIndex >= 2;
}

/// <summary>Whether Act 4 is offered in Standard runs.</summary>
public static class Act4Access
{
	/// <summary>Off until the Act 4 package is released (WP6); the owner previews it with the Act4Preview config key.</summary>
	public const bool Released = false;

	public static bool InStandardRun(bool unlocked, bool preview) => (Released || preview) && unlocked;
}
