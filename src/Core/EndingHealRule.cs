namespace SlaySpoon.Core;

/// <summary>
/// Entering The Ending heals like an act's Ancient (vanilla <c>AncientEventModel.BeforeEventStarted</c>): the missing HP,
/// 80 % of it under the Weary Traveler ascension. The Ending has no Ancient, so without this the team entered act 4 with
/// the HP left after the act 3 boss (owner report, 2026-11-05).
/// </summary>
public static class EndingHealRule
{
	public const decimal WearyTravelerFactor = 0.8m;

	public static decimal Amount(int maxHp, int currentHp, bool wearyTraveler)
	{
		decimal missing = Math.Max(0, maxHp - currentHp);
		return wearyTraveler ? missing * WearyTravelerFactor : missing;
	}
}
