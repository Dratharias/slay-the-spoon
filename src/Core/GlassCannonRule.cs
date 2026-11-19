namespace SlaySpoon.Core;

/// <summary>Frenzy's per-combat state: stacks (one per card played), the clean-turn streak, and whether a Block Skill was played this turn.</summary>
public readonly record struct FrenzyState(int Stacks, int Streak, bool BlockThisTurn)
{
	public static FrenzyState Start => new(0, 0, false);
}

/// <summary>Glass Cannon: the numbers and pure rules (docs/superpowers/specs/2026-11-19-glass-cannon-design.md).</summary>
public static class GlassCannonRule
{
	/// <summary>Damage bonus per Frenzy stack, linear like vanilla Slow.</summary>
	public const decimal BonusPerStack = 0.10m;

	/// <summary>Frail the Glass Heart applies at the start of every combat.</summary>
	public const int FrailAmount = 99;

	/// <summary>Relic ids (model id entries) every player starts with, next to the Glass Heart.</summary>
	public static readonly IReadOnlyList<string> StartRelics = ["VAJRA", "GIRYA"];

	public static decimal DamageMultiplier(int stacks) => 1m + BonusPerStack * Math.Max(0, stacks);

	/// <summary>Only a card that gains Block and is neither an Attack nor a Power breaks Frenzy.</summary>
	public static bool BreaksFrenzy(bool isAttack, bool isPower, bool blockCard) => blockCard && !isAttack && !isPower;

	/// <summary>A card was played: one more stack, or everything back to 0 when it breaks Frenzy.</summary>
	public static FrenzyState CardPlayed(FrenzyState s, bool breaks) =>
		breaks ? new FrenzyState(0, 0, true) : s with { Stacks = s.Stacks + 1 };

	/// <summary>The holder's turn ended: a clean turn extends the streak and gives Strength equal to it.</summary>
	public static (FrenzyState Next, int Strength) TurnEnded(FrenzyState s)
	{
		if (s.BlockThisTurn) return (s with { Streak = 0, BlockThisTurn = false }, 0);
		int streak = s.Streak + 1;
		return (s with { Streak = streak }, streak);
	}
}
