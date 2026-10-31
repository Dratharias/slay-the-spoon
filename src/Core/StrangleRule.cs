namespace SlaySpoon.Core;

/// <summary>
/// Strangle (and StS1 Choke, which reuses its power) in co-op: every player's cards trigger it, not only its
/// applier's (owner request, 2026-10-30). Custom Run only, so Standard and Daily keep vanilla's rule.
/// </summary>
public static class StrangleRule
{
	public static bool CountsAllies(bool customRun, int playerCount) => customRun && playerCount > 1;
}
