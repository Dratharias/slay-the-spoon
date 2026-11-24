namespace SlaySpoon.Core;

/// <summary>
/// The Witching Hour, the mod's own curse (owner, 2026-11-23): Eternal, Innate, Unplayable; it replaces Ascender's Bane in
/// Overcursed runs. On odd turns (rounds count from 1) it cannot be played even with the Unholy Grimoire, makes the owner's
/// other cards cost 1 more while it is in hand, and a copy in the draw pile is drawn 80 % of the time. Played with the
/// Grimoire (even turns), the chosen enemy skips its next turn.
/// Forged (The Forge + Cursed Deck, owner 2026-11-24): it fuses and can be Duplicated despite Eternal. The cost penalty
/// becomes 2 from +5; held in hand at the end of an odd turn it costs its owner 15 HP at +1, then 9 more per level (87 at
/// +9, above Bad Luck+9's 76); its Grimoire stun lasts 2 turns from +5. It never draws the Odd, Even or Echo defect.
/// </summary>
public static class WitchingHourRule
{
	public const string Id = "SPOON_WITCHING_HOUR";

	/// <summary>Chance, in percent, that it is pulled from the draw pile at the start of an odd turn.</summary>
	public const int DrawChance = 80;

	/// <summary>Extra energy the owner's other cards cost on odd turns while it is in hand (+0 to +4).</summary>
	public const int CostPenalty = 1;

	/// <summary>Forge level from which the cost penalty and the stun step up to their cap.</summary>
	public const int StepLevel = 5;

	/// <summary>Cap of the cost penalty and of the stun (owner: +2 cost on every card in hand is already savage).</summary>
	public const int MaxStep = 2;

	public const int ForgedHpLoss = 15;
	public const int ForgedHpLossPerLevel = 9;

	public static int CostPenaltyAt(int level) => level >= StepLevel ? MaxStep : CostPenalty;

	public static int StunTurnsAt(int level) => level >= StepLevel ? MaxStep : 1;

	/// <summary>End of an odd turn in hand: nothing unless forged; 15 at +1, then 9 more per level.</summary>
	public static int HpLossAt(int level, bool forged) =>
		forged && level >= 1 ? ForgedHpLoss + ForgedHpLossPerLevel * (level - 1) : 0;

	/// <summary>Its odd-turn rule already blocks it: Even would block it forever, Odd adds nothing; Echo has no half of a stun.</summary>
	public const CurseDefect ExcludedDefects = CurseDefect.Odd | CurseDefect.Even | CurseDefect.Echo;

	public static bool IsOddTurn(int round) => round % 2 == 1;

	/// <summary>Unplayable on odd turns, Grimoire or not.</summary>
	public static bool BlocksPlay(int round) => IsOddTurn(round);

	/// <summary>Cost of another card of the owner: the penalty on odd turns while the curse is in hand; X and unplayable costs untouched.</summary>
	public static decimal CostWith(decimal cost, int round, bool curseInHand, int level = 0) =>
		curseInHand && IsOddTurn(round) && cost >= 0 ? cost + CostPenaltyAt(level) : cost;

	/// <summary>Whether it is pulled into the hand: odd turn after the first (Innate covers turn 1), roll 0..99 under 80.</summary>
	public static bool Pulls(int round, int roll) => round > 1 && IsOddTurn(round) && roll < DrawChance;

	public static string PullStream(int floor, int round, int drawIndex) => $"witching_hour_{floor}_{round}_{drawIndex}";
}
