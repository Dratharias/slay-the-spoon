namespace SlaySpoon.Core;

/// <summary>
/// Darv's Dusty Tome for a player whose pool has no Ancient card (the Watcher): the Tome is replaced by one more of
/// Darv's relics, as in Darv's vanilla branch without the Tome.
/// </summary>
public static class DarvTomeRule
{
	/// <summary>
	/// Index in <paramref name="candidates"/> of the replacement relic: one not already <paramref name="offered"/>,
	/// chosen by <paramref name="roll"/> ≥ 0; -1 when every candidate is offered already.
	/// </summary>
	public static int Replacement(IReadOnlyList<string> candidates, IReadOnlyCollection<string> offered, int roll)
	{
		var free = Enumerable.Range(0, candidates.Count).Where(i => !offered.Contains(candidates[i])).Distinct().ToList();
		return free.Count == 0 ? -1 : free[roll % free.Count];
	}
}
