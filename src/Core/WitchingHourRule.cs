namespace SlaySpoon.Core;

/// <summary>
/// The Witching Hour, the mod's own curse (owner, 2026-11-23): Eternal, Innate, Unplayable; it replaces Ascender's Bane in
/// Overcursed runs. On odd turns (rounds count from 1) it cannot be played even with the Unholy Grimoire, makes the owner's
/// other cards cost 1 more while it is in hand, and a copy in the draw pile is drawn 80 % of the time. Played with the
/// Grimoire (even turns), the chosen enemy skips its next turn.
/// </summary>
public static class WitchingHourRule
{
	public const string Id = "SPOON_WITCHING_HOUR";

	/// <summary>Chance, in percent, that it is pulled from the draw pile at the start of an odd turn.</summary>
	public const int DrawChance = 80;

	/// <summary>Extra energy the owner's other cards cost on odd turns while it is in hand.</summary>
	public const int CostPenalty = 1;

	public static bool IsOddTurn(int round) => round % 2 == 1;

	/// <summary>Unplayable on odd turns, Grimoire or not.</summary>
	public static bool BlocksPlay(int round) => IsOddTurn(round);

	/// <summary>Cost of another card of the owner: +1 on odd turns while the curse is in hand; X and unplayable costs untouched.</summary>
	public static decimal CostWith(decimal cost, int round, bool curseInHand) =>
		curseInHand && IsOddTurn(round) && cost >= 0 ? cost + CostPenalty : cost;

	/// <summary>Whether it is pulled into the hand: odd turn after the first (Innate covers turn 1), roll 0..99 under 80.</summary>
	public static bool Pulls(int round, int roll) => round > 1 && IsOddTurn(round) && roll < DrawChance;

	public static string PullStream(int floor, int round, int drawIndex) => $"witching_hour_{floor}_{round}_{drawIndex}";
}
